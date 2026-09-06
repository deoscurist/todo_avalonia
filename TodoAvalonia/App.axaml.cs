using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TodoAvalonia.Data;
using TodoAvalonia.Data.DB;
using TodoAvalonia.Stores;
using TodoAvalonia.ViewModels;
using TodoAvalonia.Views;

namespace TodoAvalonia;

public partial class App : Application
{
    public static AppSettingStore AppSettingStore { get; private set; } = null!;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<TasksDatabase>();                                                                                                                                                                          
        services.AddSingleton<AppSettingsDatabase>();
        services.AddSingleton<IAppSettingRepository, AppSettingRepository>();
        services.AddSingleton<ITaskListRepository, TaskListRepository>();
        services.AddSingleton<AppSettingStore, AppSettingStore>();
        services.AddSingleton<TaskListStore, TaskListStore>();
        services.AddSingleton<TaskListIndexViewModel>();
        services.AddSingleton<MainViewModel>();

        return services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var provider = ConfigureServices();
        
        AppSettingStore = provider.GetRequiredService<AppSettingStore>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = provider.GetRequiredService<MainViewModel>();
            var taskListIndex = provider.GetRequiredService<TaskListIndexViewModel>();

            mainViewModel.MainContent = taskListIndex;

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };

            desktop.ShutdownRequested += (_, _) => (provider as IDisposable)?.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}