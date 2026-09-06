using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TodoAvalonia.Models;

namespace TodoAvalonia.ViewModels;

public partial class TaskItemViewModel : ViewModelBase
{
    public readonly TaskItem TaskItem;
    
    [ObservableProperty] public partial string? Content { get; set; }
    [ObservableProperty] public partial bool IsDone { get; set; }

    public TaskItemViewModel()
    {
        TaskItem = new TaskItem();
        Content = TaskItem.Content;
        IsDone = TaskItem.IsDone;
    }

    public TaskItemViewModel(TaskItem taskItem)
    {
        TaskItem = taskItem;
        Content = taskItem.Content;
        IsDone = taskItem.IsDone;
    }

    public TaskItem Commit()
    {
        TaskItem.Content = Content;
        TaskItem.IsDone = IsDone;

        return TaskItem;
    }
}
