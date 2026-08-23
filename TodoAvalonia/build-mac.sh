#!/bin/bash
set -e

APP_NAME="TodoAvalonia"
BUNDLE_ID="com.deoscurist.todoavalonia"
CSPROJ="./TodoAvalonia.csproj"
OUTPUT_DIR="$(pwd)/Publish"

# --- Версия берётся из .csproj (<Version>...</Version>) ---
VERSION=$(dotnet msbuild "$CSPROJ" -nologo -getProperty:Version)
echo "📦 Версия проекта: $VERSION"

ARCHS=("x64" "arm64")

for ARCH in "${ARCHS[@]}"; do
    echo ""
    echo "=== 🍎 Архитектура: $ARCH ==="

    PUBLISH_DIR="./bin/Release/net10.0/osx-$ARCH/publish"
    DMG_SUFFIX="-$ARCH"

    # --- Сборка ---
    echo "🔨 dotnet publish для $ARCH..."
    dotnet publish -c Release -r "osx-$ARCH" --self-contained -o "$PUBLISH_DIR"

    # --- Временная структура: .app + симлинк на /Applications рядом ---
    TMP_DIR=$(mktemp -d)
    STAGE_DIR="$TMP_DIR/stage"
    APP_BUNDLE="$STAGE_DIR/$APP_NAME.app"
    DMG_FILE="$OUTPUT_DIR/${APP_NAME}-${VERSION}${DMG_SUFFIX}.dmg"

    mkdir -p "$APP_BUNDLE/Contents/MacOS" "$APP_BUNDLE/Contents/Resources"
    cp -R "$PUBLISH_DIR"/* "$APP_BUNDLE/Contents/MacOS/"
    chmod 755 "$APP_BUNDLE/Contents/MacOS/$APP_NAME"

    # --- Info.plist ---
    cat > "$APP_BUNDLE/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>$APP_NAME</string>
    <key>CFBundleIdentifier</key>
    <string>$BUNDLE_ID</string>
    <key>CFBundleVersion</key>
    <string>$VERSION</string>
    <key>CFBundleShortVersionString</key>
    <string>$VERSION</string>
    <key>CFBundleExecutable</key>
    <string>$APP_NAME</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleSupportedPlatforms</key>
    <array><string>MacOSX</string></array>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
</dict>
</plist>
EOF

    # --- Иконка (если есть) ---
    if [ -f "./$APP_NAME.icns" ]; then
        cp "./$APP_NAME.icns" "$APP_BUNDLE/Contents/Resources/"
    fi

    # --- ОБЯЗАТЕЛЬНАЯ ПОДПИСЬ ad-hoc (без неё macOS блокирует запуск) ---
    echo "🔏 Подписываю .app (ad-hoc)..."
    codesign --force --deep --sign - "$APP_BUNDLE"

    # --- Симлинк на /Applications рядом с .app — для drag-to-install ---
    ln -s /Applications "$STAGE_DIR/Applications"

    # --- Создаём .dmg из директории с .app + симлинком ---
    mkdir -p "$OUTPUT_DIR"
    hdiutil create -volname "$APP_NAME" -srcfolder "$STAGE_DIR" -ov "$DMG_FILE" -format UDZO

    # --- Очистка ---
    rm -rf "$TMP_DIR"

    echo "✅ Готово: $DMG_FILE"
done

echo ""
echo "🎉 Все архитектуры собраны: $OUTPUT_DIR"
echo ""
echo "🔧 Если при открытии пишет 'повреждён', выполни:"
echo "   xattr -d com.apple.quarantine /путь/к/установщику"
echo "   (или перетащи .app в Приложения и открой через контекстное меню с Ctrl)"
