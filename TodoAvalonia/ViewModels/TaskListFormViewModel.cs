using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Langs;
using TodoAvalonia.Messages;
using TodoAvalonia.Models;

namespace TodoAvalonia.ViewModels;

public partial class TaskListFormViewModel : ViewModelBase
{
    public TaskList TaskListObject { get; }
    public string ModalTitle { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial string? Title { get; set; }

    [NotifyCanExecuteChangedFor(nameof(SaveCommand))] [ObservableProperty] public partial DateTimeOffset? ExpiredAtDate { get; set; }
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))] [ObservableProperty] public partial TimeSpan? ExpiredAtTime { get; set; }

    public ObservableCollection<TaskItemViewModel> Items { get; } = new();

    public TaskListFormViewModel(TaskList? taskList = null)
    {
        ModalTitle = taskList != null                                                                                                                                                                            
            ? string.Format(Localizer.Instance["Form.EditTitle"], taskList.Title)                                                                                                                                
            : Localizer.Instance["Form.CreateTitle"];
        
        TaskListObject = taskList ?? new();
        Title = taskList?.Title;
        ExpiredAtDate = taskList?.ExpiredAt;
        ExpiredAtTime = taskList?.ExpiredAt?.TimeOfDay;

        if (taskList == null)
            TrackItem(new TaskItemViewModel());

        foreach (var item in taskList?.TaskItems ?? [])
            TrackItem(new TaskItemViewModel(item));
    }

    private void TrackItem(TaskItemViewModel item)
    {
        item.PropertyChanged += (_, _) => SaveCommand.NotifyCanExecuteChanged();
        Items.Add(item);
    }

    [RelayCommand] private void AddItem() => TrackItem(new TaskItemViewModel());

    [RelayCommand]
    private void RemoveItem(TaskItemViewModel item)
    {
        if (Items.Count <= 1) return;
        Items.Remove(item);
        SaveCommand.NotifyCanExecuteChanged();
    }

    private bool CanSave() =>
        !string.IsNullOrWhiteSpace(Title)
        && Items.Any(i => !string.IsNullOrWhiteSpace(i.Content))
        && (ExpiredAtDate is null) == (ExpiredAtTime is null);

    [RelayCommand(CanExecute = nameof(CanSave))]
    public void Save()
    {
        TaskListObject.Title = Title;

        if (ExpiredAtDate != null && ExpiredAtTime != null)
        {
            TaskListObject.ExpiredAt = new DateTimeOffset(
                ExpiredAtDate.Value.Date + ExpiredAtTime.Value, ExpiredAtDate.Value.Offset).UtcDateTime;
        }
        TaskListObject.TaskItems = Items
            .Where(i => !string.IsNullOrWhiteSpace(i.Content))
            .Select(i => i.Commit())
            .ToList();

        WeakReferenceMessenger.Default.Send(new TaskListSavedMessage(TaskListObject));
        WeakReferenceMessenger.Default.Send(new CloseModalMessage());
    }
}
