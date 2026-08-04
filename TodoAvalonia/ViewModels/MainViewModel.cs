using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Data;
using TodoAvalonia.Messages;
using TodoAvalonia.Models;

namespace TodoAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] public partial object? ModalContent { get; set; }
    [ObservableProperty] public partial object? MainContent { get; set; }
    [ObservableProperty] public partial object? Notification { get; set; }

    public MainViewModel()
    {
        WeakReferenceMessenger.Default.Register<MainViewModel, ShowModalMessage>(this, (r, m) => r.ModalContent = m.Content);                                                                                                           
        WeakReferenceMessenger.Default.Register<MainViewModel, CloseModalMessage>(this, (r, m) => r.ModalContent = null); 
    }
}
