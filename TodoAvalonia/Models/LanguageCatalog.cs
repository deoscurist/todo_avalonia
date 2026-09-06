using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace TodoAvalonia.Models;

internal static class FlagImages
{
    private static readonly Dictionary<string, Bitmap> Cache = new();

    public static Bitmap Get(string flagImageName)
    {
        if (Cache.TryGetValue(flagImageName, out var cached)) return cached;

        using var stream = AssetLoader.Open(
            new Uri($"avares://TodoAvalonia/Assets/Icons/Langs/{flagImageName}.png"));

        var bitmap = new Bitmap(stream);
        Cache[flagImageName] = bitmap;

        return bitmap;
    }
}

public record Language(string Code, string FlagImageName, string Label)
{
    public Bitmap Flag => FlagImages.Get(FlagImageName);
}

public static class LanguageCatalog
{
    public static IReadOnlyList<Language> Languages { get; } =
    [
        new Language("en", "en", "English"),
        new Language("ru", "ru", "Русский"),
        new Language("es", "es", "Español"),
        new Language("ja", "jp", "日本語"),
    ];

    public static Language DefaultLanguage() => Languages.First(l => l.Code == "en");
    public static Language? FindByCode(string code) => Languages.FirstOrDefault(l => l.Code == code);

    public static Language FromSystem() =>
        FindByCode(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) ?? DefaultLanguage();
}