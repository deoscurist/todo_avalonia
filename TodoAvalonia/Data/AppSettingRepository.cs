using System.Collections.Generic;
using System.Linq;
using LiteDB;
using TodoAvalonia.Data.DB;
using TodoAvalonia.Models;

namespace TodoAvalonia.Data;

public class AppSettingRepository(AppSettingsDatabase context) : IAppSettingRepository
{
    private readonly LiteDatabase _db = context.Database;
    private readonly ILiteCollection<AppSetting> _appSettings = context.Database.GetCollection<AppSetting>("appSettings");
    
    public IEnumerable<AppSetting> GetAll()
    {
        return _appSettings.FindAll();
    }

    public bool EnsureDefaults(IEnumerable<AppSetting> defaults)
    {
        var existedKeys = _appSettings.FindAll()
            .Select(setting => setting.Key)
            .ToList();
        
        var missedSettings = defaults                                                                                                                                                                                           
            .Where(setting => !existedKeys.Contains(setting.Key))                                                                                                                                                        
            .ToList();
        
        if (missedSettings.Count == 0) return false;
        
        _appSettings.InsertBulk(missedSettings);
        
        return true;
    }

    public bool Update(AppSetting setting)
    {
        return _appSettings.Update(setting);
    }
}