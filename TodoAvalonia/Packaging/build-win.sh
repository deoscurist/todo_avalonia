#!/bin/bash
set -euo pipefail

# Собирает одиночный .exe под ОДНУ архитектуру Windows.
#
#   ./build-win.sh <x64|arm64> [версия] [отображаемое имя]
#
# Аргументы те же, что у build-mac.sh, и значат то же самое.
#
# Windows-машина для этого не нужна: .NET публикует под чужую платформу с любой.
# Родная утилита понадобится только если дело дойдёт до установщика или подписи
# Authenticode — вот тогда задачу придётся перенести на windows-раннер.
#
# Иконку .exe этот скрипт не трогает: она берётся из <ApplicationIcon> в .csproj
# и вшивается на этапе publish.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

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
    x64|arm64) ;;
    *)
        echo "❌ Нужна архитектура: x64 или arm64" >&2
        echo "   Использование: $(basename "$0") <x64|arm64> [версия] [отображаемое имя]" >&2
        exit 1
        ;;
esac

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

# Ресурс версии в .exe принимает только цифры и точки. Снимать префикс тега здесь
# не будем: это дело того, кто знает формат тега.
if ! [[ "$VERSION" =~ ^[0-9]+(\.[0-9]+)*$ ]]; then
    echo "❌ Версия '$VERSION' не годится: нужны только цифры и точки." >&2
    exit 1
fi

# --- Отображаемое имя ---
DISPLAY_NAME="${3:-$EXEC_NAME}"
# В имя файла пробелы не пускаем: в ссылке на страницу релиза они превращаются
# в %20.
FILE_NAME="${DISPLAY_NAME// /_}"

echo ""
echo "=== 🪟 $DISPLAY_NAME $VERSION, архитектура $ARCH ==="

PUBLISH_DIR="$PROJECT_DIR/bin/Release/$TFM/win-$ARCH/publish"
EXE_FILE="$OUTPUT_DIR/${FILE_NAME}-${VERSION}-windows-${ARCH}.exe"

# --- Сборка ---
# PublishSingleFile складывает управляемые сборки внутрь .exe.
# EnableCompressionInSingleFile сжимает их там же: у одиночного файла бандл по
# умолчанию не сжат, отсюда и сотня мегабайт. AppImage и .dmg сжаты своими
# форматами, так что без этого ключа windows-сборка вдвое тяжелее остальных.
# Цена: при старте содержимое распаковывается в память — запуск чуть медленнее.
# Файлов на диск при этом не создаётся.
# IncludeNativeLibrariesForSelfExtract добавляет туда же родные библиотеки —
# без него рядом с .exe остались бы libSkiaSharp.dll и прочие, и "одним файлом"
# не получилось бы. Цена: при запуске они распаковываются во временный каталог.
echo "🔨 dotnet publish для win-$ARCH..."
dotnet publish "$CSPROJ" \
    -c Release \
    -r "win-$ARCH" \
    --self-contained \
    -p:Version="$VERSION" \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true \
    -o "$PUBLISH_DIR"

# --- Проверка, что вышел действительно один исполняемый файл ---
# Без неё молчаливый откат к обычной публикации остался бы незамеченным: скрипт
# скопировал бы .exe, а нужные ему рядом .dll — нет.
if [ ! -f "$PUBLISH_DIR/$EXEC_NAME.exe" ]; then
    echo "❌ Не найден $PUBLISH_DIR/$EXEC_NAME.exe" >&2
    exit 1
fi

STRAY=$(find "$PUBLISH_DIR" -type f ! -name "*.exe" ! -name "*.pdb" | wc -l | tr -d ' ')
if [ "$STRAY" -ne 0 ]; then
    echo "⚠️  Рядом с .exe осталось файлов: $STRAY — значит одним файлом не собралось:" >&2
    find "$PUBLISH_DIR" -type f ! -name "*.exe" ! -name "*.pdb" >&2
    exit 1
fi

# --- Раскладка ---
mkdir -p "$OUTPUT_DIR"
cp "$PUBLISH_DIR/$EXEC_NAME.exe" "$EXE_FILE"

echo ""
echo "✅ Готово: $EXE_FILE"
