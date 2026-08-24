using System;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Controls;
using TodoAvalonia.Messages;
using TodoAvalonia.Models;

namespace TodoAvalonia.ViewModels;

public partial class TaskListCardViewModel : ViewModelBase, IReorderable
{
    public TaskList TaskListObject { get; set; } = new();
    
    public TaskListCardViewModel(TaskList taskList)
    {
        TaskListObject = taskList;
    }
    
    [RelayCommand]
    private void Edit() => WeakReferenceMessenger.Default.Send(new ShowModalMessage(new TaskListFormViewModel(TaskListObject)));

    [RelayCommand]
    private void Delete()
    {
        WeakReferenceMessenger.Default.Send(new NotificationMessage("Task has been deleted"));
        WeakReferenceMessenger.Default.Send(new TaskListDeleteMessage(TaskListObject.Id));   
    }
}