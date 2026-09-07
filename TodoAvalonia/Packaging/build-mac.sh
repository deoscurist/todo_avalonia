#!/bin/bash
set -euo pipefail

# Собирает .app и .dmg под ОДНУ архитектуру macOS.
#
#   ./build-mac.sh <x64|arm64> [версия] [отображаемое имя]
#
# Архитектура обязательна: на CI под каждую идёт своя задача, и цикл по обеим
# внутри скрипта означал бы, что половина работы делается дважды.
#
# Версия необязательна. Не передали (или передали пустую строку) — берётся из
# <Version> в .csproj, это режим локальной сборки. Передали — она главнее:
# из тега её подставляет вызывающий.
#
# Отображаемое имя необязательно. Не передали — совпадает с именем сборки.
# Это вывеска: заголовок бандла, имя тома и имя .dmg. С именем исполняемого
# файла оно намеренно не связано — то диктуется .csproj и меняться от вывески
# не должно.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

BUNDLE_ID="com.deoscurist.todoavalonia"
# Выход остаётся в каталоге проекта: в .gitignore лежит "*/Publish", и звёздочка
# не проходит сквозь косую черту — в Packaging/Publish он бы уже не сработал.
OUTPUT_DIR="$PROJECT_DIR/Publish"

# --- Проект ---
# Имя файла проекта сюда не вписываем: рядом он один, и пусть его находит поиск,
# а не совпадение строки.
shopt -s nullglob
CSPROJ_LIST=("$PROJECT_DIR"/*.csproj)
shopt -u nullglob
if [ ${#CSPROJ_LIST[@]} -ne 1 ]; then
    echo "❌ В $PROJECT_DIR ожидался ровно один .csproj, найдено ${#CSPROJ_LIST[@]}" >&2
    exit 1
fi
CSPROJ="${CSPROJ_LIST[0]}"

# --- Архитектура ---
ARCH="${1:-}"
case "$ARCH" in
    # В имя файла идёт не архитектура, а то, как пользователь называет свой мак:
    # на странице релиза он опознаёт себя словами, а не через x64 и arm64.
    x64)   ARCH_LABEL="intel" ;;
    arm64) ARCH_LABEL="apple-silicon" ;;
    *)
        echo "❌ Нужна архитектура: x64 или arm64" >&2
        echo "   Использование: $(basename "$0") <x64|arm64> [версия] [отображаемое имя]" >&2
        exit 1
        ;;
esac

# --- Значения из .csproj ---
# Платформа и имя сборки объявлены в проекте. Продублированные строкой сюда, они
# разъедутся с ним при первом же изменении — и молча: каталог задаётся ключом -o
# и будет создан под любым именем, а несовпадение имени всплывёт только тем,
# что собранный бандл не запустится.
TFM=$(dotnet msbuild "$CSPROJ" -nologo -getProperty:TargetFramework | tr -d '[:space:]')
EXEC_NAME=$(dotnet msbuild "$CSPROJ" -nologo -getProperty:AssemblyName | tr -d '[:space:]')

# --- Версия ---
VERSION="${2:-}"
if [ -z "$VERSION" ]; then
    VERSION=$(dotnet msbuild "$CSPROJ" -nologo -getProperty:Version | tr -d '[:space:]')
    echo "📦 Версия из .csproj: $VERSION"
else
    echo "📦 Версия передана снаружи: $VERSION"
fi

# CFBundleVersion и CFBundleShortVersionString принимают только цифры и точки.
# Снимать префикс тега здесь не будем: это дело того, кто знает формат тега.
# Скрипт лишь отказывается собирать заведомо негодный Info.plist.
if ! [[ "$VERSION" =~ ^[0-9]+(\.[0-9]+)*$ ]]; then
    echo "❌ Версия '$VERSION' не годится для Info.plist: нужны только цифры и точки." >&2
    exit 1
fi

# --- Отображаемое имя ---
DISPLAY_NAME="${3:-$EXEC_NAME}"
# В имя файла пробелы не пускаем: в ссылке на страницу релиза они превращаются
# в %20. На вывеску это не влияет — в бандле и в Info.plist имя остаётся как
# передано, с пробелами.
FILE_NAME="${DISPLAY_NAME// /_}"

# --- Иконка ---
# Имя файла значка в Resources обязано совпадать с CFBundleIconFile, поэтому
# оно привязано к имени сборки, а не к вывеске: иначе смена вывески оставила бы
# бандл без иконки.
ICNS="$SCRIPT_DIR/macos/$EXEC_NAME.icns"
# Раньше тут стояло "если файл есть — скопировать", и при неверном пути бандл
# молча собирался без иконки, без единого сообщения. Теперь это ошибка.
if [ ! -f "$ICNS" ]; then
    echo "❌ Не найдена иконка: $ICNS" >&2
    exit 1
fi

echo ""
echo "=== 🍎 $DISPLAY_NAME $VERSION, архитектура $ARCH ==="

PUBLISH_DIR="$PROJECT_DIR/bin/Release/$TFM/osx-$ARCH/publish"
DMG_FILE="$OUTPUT_DIR/${FILE_NAME}-${VERSION}-macos-${ARCH_LABEL}.dmg"

# --- Сборка ---
echo "🔨 dotnet publish для $ARCH..."
dotnet publish "$CSPROJ" \
    -c Release \
    -r "osx-$ARCH" \
    --self-contained \
    -p:Version="$VERSION" \
    -o "$PUBLISH_DIR"

# --- Временная структура: .app + симлинк на /Applications рядом ---
TMP_DIR=$(mktemp -d)
trap 'rm -rf "$TMP_DIR"' EXIT
STAGE_DIR="$TMP_DIR/stage"
APP_BUNDLE="$STAGE_DIR/$DISPLAY_NAME.app"

mkdir -p "$APP_BUNDLE/Contents/MacOS" "$APP_BUNDLE/Contents/Resources"
cp -R "$PUBLISH_DIR"/* "$APP_BUNDLE/Contents/MacOS/"
chmod 755 "$APP_BUNDLE/Contents/MacOS/$EXEC_NAME"

# --- Иконка ---
cp "$ICNS" "$APP_BUNDLE/Contents/Resources/"

# --- Info.plist ---
# CFBundleIconFile обязателен: без него macOS не возьмёт .icns из Resources
# и покажет стандартный значок, даже когда файл лежит на месте.
cat > "$APP_BUNDLE/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>$DISPLAY_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>$DISPLAY_NAME</string>
    <key>CFBundleIdentifier</key>
    <string>$BUNDLE_ID</string>
    <key>CFBundleVersion</key>
    <string>$VERSION</string>
    <key>CFBundleShortVersionString</key>
    <string>$VERSION</string>
    <key>CFBundleExecutable</key>
    <string>$EXEC_NAME</string>
    <key>CFBundleIconFile</key>
    <string>$EXEC_NAME</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleSupportedPlatforms</key>
    <array><string>MacOSX</string></array>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>LSMinimumSystemVersion</key>
    <string>12.0</string>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
</dict>
</plist>
EOF

# --- ОБЯЗАТЕЛЬНАЯ ПОДПИСЬ ad-hoc (без неё macOS блокирует запуск) ---
#
# Ключ --deep Apple объявила устаревшим для подписи начиная с macOS 13, и здесь
# он стоит осознанно. Возражений у Apple два: --deep применяет одни и те же
# параметры ко всему вложенному сразу, и он обходит только те места, где ожидает
# найти код. Первое бьёт, когда у вложенного кода свои entitlements; второе —
# когда есть код там, где система ждёт данные. При ad-hoc подписи без
# entitlements и без нотаризации не работает ни то, ни другое.
#
# Ручная замена означала бы перечислить, что считать кодом: в самодостаточной
# публикации это две с лишним сотни управляемых .dll, полтора десятка .dylib и
# Mach-O createdump без расширения. Состав меняется вместе с публикацией .NET,
# а список в скрипте — нет.
#
# Если дойдёт до Developer ID и нотаризации — здесь нужен явный обход изнутри
# наружу: там разные параметры для разного вложенного кода действительно нужны.
echo "🔏 Подписываю .app (ad-hoc)..."
codesign --force --deep --sign - "$APP_BUNDLE"

# --- Симлинк на /Applications рядом с .app — для drag-to-install ---
ln -s /Applications "$STAGE_DIR/Applications"

# --- Создаём .dmg из директории с .app + симлинком ---
# ULMO — сжатие lzma, требует macOS 10.15+; у нас нижняя граница 12.0, проходит.
# Умолчание hdiutil — UDZO на zlib, он заметно слабее. Медленное разжатие у lzma
# человек платит один раз при монтировании, размер скачивания — каждый раз.
# Если понадобится компромисс, ULFO (lzfse) жмёт слабее, но распаковывается
# быстрее; это то же самое слово в этой строке.
mkdir -p "$OUTPUT_DIR"
hdiutil create -volname "$DISPLAY_NAME" -srcfolder "$STAGE_DIR" -ov "$DMG_FILE" -format ULMO

echo ""
echo "✅ Готово: $DMG_FILE"
echo ""
echo "🔧 Если при открытии пишет 'повреждён', выполни:"
echo "   xattr -d com.apple.quarantine /путь/к/установщику"
echo "   (или перетащи .app в Приложения и открой через контекстное меню с Ctrl)"
