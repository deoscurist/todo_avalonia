using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Data;
using TodoAvalonia.Messages;
using TodoAvalonia.Models;
using TodoAvalonia.Services;

namespace TodoAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] public partial object? ModalContent { get; set; }
    [ObservableProperty] public partial object? MainContent { get; set; }
    public NotificationService Notifications { get; } = new();
    
    public string AppVersion { get; } =                                                                                                                                                                              
        "v" + (Assembly.GetExecutingAssembly()                                                                                                                                                                       
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0"); 

    public MainViewModel()
    {
        WeakReferenceMessenger.Default.Register<MainViewModel, ShowModalMessage>(this, (r, m) => r.ModalContent = m.Content);                                                                                                           
        WeakReferenceMessenger.Default.Register<MainViewModel, CloseModalMessage>(this, (r, m) => r.ModalContent = null); 
        WeakReferenceMessenger.Default.Register<MainViewModel, NotificationMessage>(this, (r, m) => r.Notifications.Show(m.Title, m.Message, m.Type)); 
    }
}
