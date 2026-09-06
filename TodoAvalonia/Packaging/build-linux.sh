#!/bin/bash
set -euo pipefail

# Собирает .AppImage под ОДНУ архитектуру Linux.
#
#   ./build-linux.sh <x64|arm64> [версия] [отображаемое имя]
#
# Аргументы те же, что у build-mac.sh, и значат то же самое.
#
# ВАЖНО: этот скрипт работает только на Linux. Сам бинарь .NET собрал бы и с
# macOS, но appimagetool — линуксовая программа, и запустить её больше негде.
# Ровно та же причина, по которой .dmg собирается только на маке.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

ICONS_DIR="$SCRIPT_DIR/linux/icons"
# Выход остаётся в каталоге проекта: в .gitignore лежит "*/Publish", и звёздочка
# не проходит сквозь косую черту — в Packaging/Publish он бы уже не сработал.
OUTPUT_DIR="$PROJECT_DIR/Publish"

# Размеры, которые раскладываются по hicolor. Список взят из спецификации
# freedesktop; файлы под них лежат в linux/icons и называются logo_<размер>.png.
ICON_SIZES=(16 22 24 32 48 64 128 256 512)
# Этот же размер кладётся в корень AppDir — оттуда его берёт appimagetool.
ICON_ROOT_SIZE=256

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
    x64)   APPIMAGE_ARCH="x86_64" ;;
    arm64) APPIMAGE_ARCH="aarch64" ;;
    *)
        echo "❌ Нужна архитектура: x64 или arm64" >&2
        echo "   Использование: $(basename "$0") <x64|arm64> [версия] [отображаемое имя]" >&2
        exit 1
        ;;
esac

# --- Инструмент упаковки ---
if ! command -v appimagetool >/dev/null 2>&1; then
    echo "❌ Не найден appimagetool. Он линуксовый, на macOS не запустится." >&2
    echo "   На раннере ставится загрузкой релиза AppImageKit и chmod +x." >&2
    exit 1
fi

# --- Значения из .csproj ---
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

if ! [[ "$VERSION" =~ ^[0-9]+(\.[0-9]+)*$ ]]; then
    echo "❌ Версия '$VERSION' не годится: нужны только цифры и точки." >&2
    exit 1
fi

# --- Отображаемое имя ---
DISPLAY_NAME="${3:-$EXEC_NAME}"

# --- Иконки ---
# Имя значка в .desktop обязано совпадать с именем файла в корне AppDir, поэтому
# оно привязано к имени сборки, а не к вывеске: иначе смена вывески оставила бы
# ярлык без иконки. Та же логика, что с CFBundleIconFile на маке.
for SIZE in "${ICON_SIZES[@]}" ; do
    if [ ! -f "$ICONS_DIR/logo_$SIZE.png" ]; then
        echo "❌ Не найдена иконка: $ICONS_DIR/logo_$SIZE.png" >&2
        exit 1
    fi
done

echo ""
echo "=== 🐧 $DISPLAY_NAME $VERSION, архитектура $ARCH ($APPIMAGE_ARCH) ==="

PUBLISH_DIR="$PROJECT_DIR/bin/Release/$TFM/linux-$ARCH/publish"
APPIMAGE_FILE="$OUTPUT_DIR/${DISPLAY_NAME}-${VERSION}-linux-${APPIMAGE_ARCH}.AppImage"

# --- Сборка ---
echo "🔨 dotnet publish для linux-$ARCH..."
dotnet publish "$CSPROJ" \
    -c Release \
    -r "linux-$ARCH" \
    --self-contained \
    -p:Version="$VERSION" \
    -o "$PUBLISH_DIR"

# --- Структура AppDir ---
TMP_DIR=$(mktemp -d)
trap 'rm -rf "$TMP_DIR"' EXIT
APPDIR="$TMP_DIR/AppDir"

mkdir -p "$APPDIR/usr/bin" "$APPDIR/usr/share/applications"
cp -R "$PUBLISH_DIR"/* "$APPDIR/usr/bin/"
chmod 755 "$APPDIR/usr/bin/$EXEC_NAME"

# --- Иконки по темам плюс копия в корне ---
for SIZE in "${ICON_SIZES[@]}" ; do
    DEST="$APPDIR/usr/share/icons/hicolor/${SIZE}x${SIZE}/apps"
    mkdir -p "$DEST"
    cp "$ICONS_DIR/logo_$SIZE.png" "$DEST/$EXEC_NAME.png"
done
cp "$ICONS_DIR/logo_$ICON_ROOT_SIZE.png" "$APPDIR/$EXEC_NAME.png"

# --- Ярлык ---
# Пишется здесь, а не лежит в репозитории готовым, по той же причине, что и
# Info.plist на маке: в нём и вывеска, и имя исполняемого файла, и оба приходят
# аргументами. Готовый файл пришлось бы править параллельно скрипту.
#
# Ярлык нужен в двух местах: в корне AppDir его читает appimagetool, в
# usr/share/applications — рабочий стол после установки.
DESKTOP_CONTENT="[Desktop Entry]
Type=Application
Name=$DISPLAY_NAME
Exec=$EXEC_NAME
Icon=$EXEC_NAME
Categories=Utility;
Terminal=false
"
echo "$DESKTOP_CONTENT" > "$APPDIR/$EXEC_NAME.desktop"
cp "$APPDIR/$EXEC_NAME.desktop" "$APPDIR/usr/share/applications/"

# --- Точка входа ---
# AppRun запускается из смонтированного образа, поэтому путь считается от него,
# а не от текущего каталога.
cat > "$APPDIR/AppRun" <<EOF
#!/bin/sh
HERE="\$(dirname "\$(readlink -f "\$0")")"
exec "\$HERE/usr/bin/$EXEC_NAME" "\$@"
EOF
chmod 755 "$APPDIR/AppRun"

# --- Упаковка ---
# APPIMAGE_EXTRACT_AND_RUN нужен, потому что appimagetool сам поставляется как
# AppImage и без него требует FUSE, которого в раннерах обычно нет.
mkdir -p "$OUTPUT_DIR"
ARCH="$APPIMAGE_ARCH" APPIMAGE_EXTRACT_AND_RUN=1 \
    appimagetool "$APPDIR" "$APPIMAGE_FILE"

echo ""
echo "✅ Готово: $APPIMAGE_FILE"
