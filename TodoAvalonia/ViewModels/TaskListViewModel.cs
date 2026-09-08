using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Controls;
using TodoAvalonia.Langs;
using TodoAvalonia.Messages;
using TodoAvalonia.Models;
using TodoAvalonia.Views;

namespace TodoAvalonia.ViewModels;

public partial class TaskListViewModel : ViewModelBase, IReorderable
{
    public readonly TaskList TaskList;

    [ObservableProperty] public partial string? Title { get; set; }
    [ObservableProperty] public partial int? Order { get; set; }
    public DateTime CreatedAt { get; }

    [NotifyPropertyChangedFor(nameof(Status))]
    [ObservableProperty]
    public partial DateTime? ExpiredAt { get; set; }

    public ObservableCollection<TaskItemViewModel> Items { get; } = [];

    public string Status
    {
        get
        {
            if (Items.Count > 0 && Items.All(item => item.IsDone))
            {
                return "Status.Completed";
            }

            return ExpiredAt < DateTime.Now ? "Status.Outdated" : "Status.Incomplete";
        }
    }

    public TaskListViewModel(TaskList taskList)
    {
        TaskList = taskList;

        Title = taskList.Title;
        CreatedAt = taskList.CreatedAt;
        ExpiredAt = taskList.ExpiredAt;
        Order = taskList.Order;

        foreach (var taskItem in taskList.TaskItems)
        {
            var itemViewModel = new TaskItemViewModel(taskItem);
            itemViewModel.PropertyChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(Status));
                WeakReferenceMessenger.Default.Send(new TaskListSavedMessage(Commit()));
            };

            Items.Add(itemViewModel);
        }
    }

    public TaskList Commit()
    {
        TaskList.Title = Title;
        TaskList.ExpiredAt = ExpiredAt;
        TaskList.TaskItems = [.. Items.Select(i => i.Commit())];

        return TaskList;
    }

    [RelayCommand]
    private void Edit() =>
        WeakReferenceMessenger.Default.Send(new ShowModalMessage(new TaskListFormViewModel(TaskList)));

    [RelayCommand]
    private void Delete() => WeakReferenceMessenger.Default.Send(new TaskListDeleteMessage(TaskList.Id));

    [RelayCommand]
    private void Open() =>
        WeakReferenceMessenger.Default.Send(new ShowModalMessage(new TaskListShowView { DataContext = this }));

    public void NewIndex(int newIndex)
    {
        try
        {
            App.TaskListStore.Reorder(TaskList, newIndex);
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