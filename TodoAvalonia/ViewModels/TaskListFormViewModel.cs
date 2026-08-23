using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Messages;
using TodoAvalonia.Models;

namespace TodoAvalonia.ViewModels;

public partial class TaskListFormViewModel : ViewModelBase
{
    public TaskList TaskListObject { get; }
    public string ModalTitle { get; set; }
    
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]     
    
    [ObservableProperty] public partial string? Title { get; set; }
    [ObservableProperty] public partial DateTimeOffset? ExpiredAt { get; set; } 
    public ObservableCollection<TaskItem> Items { get; } = new();

    public TaskListFormViewModel(TaskList? taskList = null)
    {
        ModalTitle = taskList != null ? $"Edit \"{taskList.Title}\"" : "Create a new task";
        TaskListObject = taskList ?? new();
        Title = taskList?.Title;
        ExpiredAt = taskList?.ExpiredAt;

        if (taskList == null)
            Items.Add(new TaskItem());
        
        foreach (var item in taskList?.TaskItems ?? [])                                                                                                                                                                  
            Items.Add(item);  
    }
    
    [RelayCommand] private void AddItem() => Items.Add(new TaskItem { Id = Guid.NewGuid(), Content = "" });
    [RelayCommand] private void RemoveItem(TaskItem item) => Items.Remove(item);
    
    private bool CanSave() => !string.IsNullOrWhiteSpace(Title); 

    [RelayCommand(CanExecute = nameof(CanSave))] public void Save()
    {
        TaskListObject.Title = Title;
        TaskListObject.ExpiredAt = ExpiredAt?.UtcDateTime;
        TaskListObject.TaskItems = Items;
            
        WeakReferenceMessenger.Default.Send(new NotificationMessage("Task has been saved"));
        WeakReferenceMessenger.Default.Send(new TaskListSavedMessage(TaskListObject));
        WeakReferenceMessenger.Default.Send(new CloseModalMessage());
    }
}