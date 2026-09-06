using System;
using System.IO;
using LiteDB;

namespace TodoAvalonia.Data.DB;

public abstract class DatabaseContext : IDisposable
{
    public LiteDatabase Database { get; }

    protected DatabaseContext(string fileName)
    {
        var appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        
        Directory.CreateDirectory(
            Path.Combine(appPath, "TodoAvalonia")
        );
        
        var path = Path.Combine(appPath, "TodoAvalonia", fileName);
        
        Database = new LiteDatabase(path) { UtcDate = true };
    }
    
    public void Dispose() => Database.Dispose();
}

public sealed class TasksDatabase() : DatabaseContext("tasks.db");
public sealed class AppSettingsDatabase() : DatabaseContext("app_settings.db");