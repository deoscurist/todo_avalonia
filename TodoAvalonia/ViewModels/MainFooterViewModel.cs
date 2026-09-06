using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using TodoAvalonia.Langs;
using TodoAvalonia.Models;
using TodoAvalonia.Stores;

namespace TodoAvalonia.ViewModels;

public partial class MainFooterViewModel : ViewModelBase
{
    private readonly AppSettingStore _store;
    public IReadOnlyList<Language> SelectableLanguages { get; } = LanguageCatalog.Languages;
    private readonly AppSetting _languageSetting;
    [ObservableProperty] private Language _currentLanguage;

    public string AppVersion { get; } =
        "v" + (Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0");

    public MainFooterViewModel(AppSettingStore store)
    {
        _store = store;

        _languageSetting = store.AppSettings.First(setting => setting.Key == "lang");

        _currentLanguage = LanguageCatalog.FindByCode(_languageSetting.Value)
                           ?? LanguageCatalog.DefaultLanguage();
    }

    partial void OnCurrentLanguageChanged(Language value)
    {
        _languageSetting.Value = value.Code;

        if (_store.Update(_languageSetting))
            Localizer.Instance.SetLanguage(value.Code);
    }
}