# Образ для сборки .AppImage с машины, на которой нет Linux.
#
# Раннеру он не нужен: ubuntu-latest сам линуксовый, там build-linux.sh идёт
# напрямую, а appimagetool ставится отдельным шагом. Это инструмент для местной
# работы — собрать линуксовый артефакт, не имея Linux под рукой.

FROM mcr.microsoft.com/dotnet/sdk:10.0

# desktop-file-utils нужен самому appimagetool: он зовёт desktop-file-validate
# и без него отказывается проверять ярлык. file — для определения типов.
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
        curl \
        desktop-file-utils \
        file \
 && rm -rf /var/lib/apt/lists/*

# appimagetool публикуется под катящимся тегом continuous — отдельных версий у
# него нет, закрепиться не за что. Адрес вынесен в ARG: под arm64 подставляется
# appimagetool-aarch64.AppImage. Если понадобится воспроизводимость — сюда
# добавляется сверка контрольной суммы, тег её не даёт.
ARG APPIMAGETOOL_URL=https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage
RUN curl -fsSL -o /usr/local/bin/appimagetool "$APPIMAGETOOL_URL" \
 && chmod +x /usr/local/bin/appimagetool

# appimagetool сам поставляется как AppImage и без FUSE не запускается, а FUSE в
# контейнере нет. Переменная заставляет его распаковаться и отработать без него.
ENV APPIMAGE_EXTRACT_AND_RUN=1

# Каталог проекта монтируется сюда, поэтому build-linux.sh находит и .csproj,
# и свои иконки по обычным относительным путям.
WORKDIR /src
ENTRYPOINT ["bash", "Packaging/build-linux.sh"]
