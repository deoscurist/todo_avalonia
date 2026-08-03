using System;
using System.IO;

namespace TodoAvalonia.Data;

public static class StoragePathProvider
{
    private static string AppPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }

    public static string AppTasksDatabasePath()
    {
        var appPath = AppPath();
        
        Directory.CreateDirectory(
            Path.Combine(appPath, "TodoAvalonia")
            );
        
        return Path.Combine(appPath, "TodoAvalonia", "tasks.db");
    }
}