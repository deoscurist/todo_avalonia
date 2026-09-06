using System.Collections.ObjectModel;
using TodoAvalonia.Data;
using TodoAvalonia.Models;

namespace TodoAvalonia.Stores;

public class AppSettingStore
{
    private readonly IAppSettingRepository _repository;
    private readonly ObservableCollection<AppSetting> _appSettings;
    public ReadOnlyObservableCollection<AppSetting> AppSettings { get; }
    
    public AppSettingStore(IAppSettingRepository repository)
    {
        _repository = repository;

        repository.EnsureDefaults([
            new AppSetting {Key = "lang", Value = LanguageCatalog.FromSystem().Code}
        ]);
        
        _appSettings = new ObservableCollection<AppSetting>(repository.GetAll());
        AppSettings = new ReadOnlyObservableCollection<AppSetting>(_appSettings);
    }
    
    public bool Update(AppSetting setting) => _repository.Update(setting);
}