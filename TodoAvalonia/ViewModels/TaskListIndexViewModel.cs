using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Data;
using TodoAvalonia.Langs;
using TodoAvalonia.Messages;
using TodoAvalonia.Models;
using TodoAvalonia.Stores;

namespace TodoAvalonia.ViewModels;

public partial class TaskListIndexViewModel : ViewModelBase
{
    private readonly TaskListStore _store;
    public ObservableCollection<object> ListItems { get; } = new();

    public TaskListIndexViewModel(TaskListStore store)
    {
        _store = store;

        foreach (var taskList in _store.TaskLists)
        {
            ListItems.Add(new TaskListViewModel(taskList));
        }

        ((INotifyCollectionChanged)_store.TaskLists).CollectionChanged += OnTaskListsChanged;

        RegisterMessages();
    }

    private void OnTaskListsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        if (e.NewItems[i] is TaskList newItem)
                        {
                            int targetIndex = e.NewStartingIndex + i;
                            ListItems.Insert(targetIndex, new TaskListViewModel(newItem));
                        }
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldStartingIndex >= 0)
                {
                    ListItems.RemoveAt(e.OldStartingIndex + 1);
                }
                break;

            case NotifyCollectionChangedAction.Move:
                if (e.OldStartingIndex >= 0 && e.NewStartingIndex >= 0)
                {
                    ListItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                if (e.NewItems?[0] is TaskList replacedItem && e.OldStartingIndex >= 0)
                {
                    ListItems[e.OldStartingIndex] = new TaskListViewModel(replacedItem);
                }
                break;
        }
    }

    private void RegisterMessages()
    {
        WeakReferenceMessenger.Default.Register<TaskListIndexViewModel, TaskListSavedMessage>(this, (r, m) =>
        {
            try
            {
                var existing = r._store.TaskLists.FirstOrDefault(t => t.Id == m.List.Id);
                if (existing == null)
                {
                    r._store.Create(m.List);
                }
                else
                {
                    r._store.Update(m.List);
                }
                WeakReferenceMessenger.Default.Send(new NotificationMessage(
                    Title: Localizer.Instance["Success.TaskSaved"],
                    Type: "success"
                    ));
            }
            catch (Exception e)
            {
                WeakReferenceMessenger.Default.Send(new NotificationMessage(
                    Title: e.Message,
                    Type: "error"
                    ));
            }
        });

        WeakReferenceMessenger.Default.Register<TaskListIndexViewModel, TaskListDeleteMessage>(this, (r, m) =>
        {
            try
            {
                var model = r._store.TaskLists.FirstOrDefault(t => t.Id == m.Id);
                if (model != null)
                {
                    r._store.Delete(model);
                }

                WeakReferenceMessenger.Default.Send(new NotificationMessage(
                    Title: Localizer.Instance["Success.TaskDeleted"],
                    Type: "success"
                    ));
            }
            catch (Exception e)
            {
                WeakReferenceMessenger.Default.Send(new NotificationMessage(
                    Title: e.Message,
                    Type: "error"
                    ));
            }
        });

        WeakReferenceMessenger.Default.Register<TaskListIndexViewModel, TaskListReorderMessage>(this, (r, m) =>
        {
            if (m.Dragged is not TaskListViewModel dragged) return;

            var model = r._store.TaskLists.FirstOrDefault(t => t.Id == dragged.TaskList.Id);
            if (model is null) return;

            int actualStoreIndex = Math.Max(0, m.NewIndex - 1); 
            r._store.Reorder(model, actualStoreIndex);
        });
    }
}