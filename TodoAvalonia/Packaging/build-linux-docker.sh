#!/bin/bash
set -euo pipefail

# Собирает .AppImage в контейнере — для машин, на которых нет Linux.
#
#   ./build-linux-docker.sh <x64> [версия] [отображаемое имя]
#
# Аргументы те же, что у build-linux.sh, и уходят ему как есть.
# На раннере этот скрипт не нужен: ubuntu-latest линуксовый, там build-linux.sh
# запускается напрямую.
#
# Образ собирается один раз при первом запуске. Пересобрать — удалить его
# командой docker rmi и запустить снова.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

IMAGE="todoavalonia-appimage-builder"
BUILD_CONTEXT="$SCRIPT_DIR/linux"
# Отдельный том под кэш пакетов: без него каждый запуск заново тянет NuGet.
NUGET_VOLUME="todoavalonia-nuget"

# --- Архитектура ---
# Проверяется здесь, а не внутри build-linux.sh: в образ положен appimagetool
# под x86_64, и под arm64 контейнер упал бы уже на упаковке, потратив сборку.
ARCH="${1:-}"
if [ "$ARCH" != "x64" ]; then
    echo "❌ В образе лежит appimagetool под x86_64, поддерживается только x64." >&2
    echo "   Для arm64 образ надо пересобрать, подставив в ARG APPIMAGETOOL_URL" >&2
    echo "   адрес appimagetool-aarch64.AppImage." >&2
    echo "   Использование: $(basename "$0") x64 [версия] [отображаемое имя]" >&2
    exit 1
fi

if ! command -v docker >/dev/null 2>&1; then
    echo "❌ docker не найден в PATH." >&2
    exit 1
fi

if ! docker info >/dev/null 2>&1; then
    echo "❌ docker установлен, но демон не отвечает — запусти Docker Desktop." >&2
    exit 1
fi

# --- Образ ---
if ! docker image inspect "$IMAGE" >/dev/null 2>&1; then
    echo "🐳 Образа нет, собираю. Это один раз и примерно 800 МБ."
    docker build --platform linux/amd64 -t "$IMAGE" "$BUILD_CONTEXT"
fi

# --- Сборка ---
# Монтируется каталог проекта, а не корень репозитория: build-linux.sh считает
# пути от себя, и Publish попадёт туда же, куда при местной сборке.
echo "🐳 Запускаю сборку в контейнере..."
docker run --rm \
    --platform linux/amd64 \
    -v "$PROJECT_DIR":/src \
    -v "$NUGET_VOLUME":/root/.nuget/packages \
    "$IMAGE" "$@"
