using System.Collections.Generic;
using TodoAvalonia.Models;

namespace TodoAvalonia.Data;

public interface IAppSettingRepository
{
    IEnumerable<AppSetting> GetAll();
    bool EnsureDefaults(IEnumerable<AppSetting> defaults);
    bool Update(AppSetting setting);
}