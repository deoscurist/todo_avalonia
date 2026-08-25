using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Data;
using TodoAvalonia.Messages;
using System.Collections.Specialized;
using System.Linq;
using TodoAvalonia.Models;

namespace TodoAvalonia.ViewModels;

public partial class TaskListIndexViewModel : ViewModelBase
{
    private readonly TaskListStore _store;
    public ObservableCollection<object> ListItems { get; } = new();
    
    private void CollectListItems()
    {
        ListItems.Add(new CreateTaskViewModel());

        foreach (var taskList in _store.TaskLists)
            ListItems.Add(new TaskListCardViewModel(taskList));
    }

    public TaskListIndexViewModel(TaskListStore store)
    {
        _store = store;
        
        CollectListItems();
        
        _store.TaskLists.CollectionChanged += (_, e) =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ListItems.Insert(e.NewStartingIndex + 1, new TaskListCardViewModel((TaskList)e.NewItems![0]!));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ListItems.RemoveAt(e.OldStartingIndex + 1);
                    break;
                case NotifyCollectionChangedAction.Move:
                    ListItems.Move(e.OldStartingIndex + 1, e.NewStartingIndex + 1);
                    break;
            }
        };
        
        WeakReferenceMessenger.Default.Register<TaskListIndexViewModel, TaskListReorderMessage>(this, (r, m) =>
        {
            if (m.Dragged is not TaskListCardViewModel dragged) return;

            r._store.Move(dragged.TaskListObject, m.NewIndex);
        });
    }
}