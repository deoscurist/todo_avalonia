using System;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Messages;
using TodoAvalonia.Models;

namespace TodoAvalonia.ViewModels;

public partial class TaskCardViewModel : ViewModelBase
{
    public TaskList TaskListObject { get; set; } = new();
    
    public TaskCardViewModel(TaskList taskList)
    {
        TaskListObject = taskList;
    }

    [RelayCommand]
    private void Delete(Guid id)
    {
        WeakReferenceMessenger.Default.Send(new NotificationMessage("Task has been deleted"));
        WeakReferenceMessenger.Default.Send(new TaskListDeleteMessage(id));   
    }
}