using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Messages;

namespace TodoAvalonia.ViewModels;

public partial class TaskListFilterViewModel : ViewModelBase
{
    [NotifyCanExecuteChangedFor(nameof(ReorderAllCommand))] 
    [ObservableProperty] 
    public partial string FilterBy { get; set; } = string.Empty;
    
    [RelayCommand] public void SetFilterBy(string filterBy) => FilterBy = filterBy;
    
    private bool CanReorder() => FilterBy != string.Empty;

    [RelayCommand(CanExecute = nameof(CanReorder))]
    public void ReorderAll(string direction)
    {
        try
        {
            var descending = direction == "desc";
            
            App.TaskListStore.ReorderAll(descending, FilterBy);
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(new NotificationMessage(
                Title: e.Message,
                Type: "error"
            ));
        }
    }
}