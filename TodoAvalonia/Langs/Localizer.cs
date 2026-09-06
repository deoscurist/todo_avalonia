using System;
using System.Globalization;
using System.Resources;
using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TodoAvalonia.Langs;

public class Localizer : ObservableObject
{
    public static Localizer Instance { get; } = new();

    private Localizer()
    {
    }

    private readonly ResourceManager _resources =
        new("TodoAvalonia.Langs.Strings", typeof(Localizer).Assembly);

    public string this[string key] => _resources.GetString(key) ?? key;
    
    public string CurrentCode { get; private set; } = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    public void SetLanguage(string code)
    {
        var culture = new CultureInfo(code);

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        
        CurrentCode = code;                                                                                                                                                                                      
        OnPropertyChanged(nameof(CurrentCode)); 
        OnPropertyChanged("Item");
    }
}

public class Tr                                                                                                                                                                                                  
{                                                                                                                                                                                                                
    public Tr(string key) => Key = key;                                                                                                                                                                          
                                                                                                                                                                                                                   
    public string Key { get; set; }                                                                                                                                                                              
                                                                                                                                                                                                                   
    public BindingBase ProvideValue(IServiceProvider serviceProvider)                                                                                                                                            
        => new Binding($"[{Key}]") { Source = Localizer.Instance };                                                                                                                                              
}    