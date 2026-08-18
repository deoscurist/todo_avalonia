using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Data;
using TodoAvalonia.Messages;
using System.Collections.Specialized;

namespace TodoAvalonia.ViewModels;

public partial class TaskListIndexViewModel : ViewModelBase
{
    private readonly ITaskListRepository _repository;
    public ObservableCollection<TaskCardViewModel> TaskLists { get; set; } = new();
    public ObservableCollection<object> ListItems { get; } = new();
    
    private void RefreshLists()
    {
        TaskLists.Clear();
        foreach (var taskList in _repository.GetAll())
            TaskLists.Add(new TaskCardViewModel(taskList));
        CollectListItems();
    }
    
    private void CollectListItems()
    {
        ListItems.Clear();
        ListItems.Add(new CreateTaskViewModel());

        foreach (var taskList in TaskLists)
            ListItems.Add(taskList);
    }

    public TaskListIndexViewModel(ITaskListRepository repository)
    {
        _repository = repository;
        RefreshLists();
        
        TaskLists.CollectionChanged += (_, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                ListItems.Move(e.OldStartingIndex + 1, e.NewStartingIndex + 1);
            else
                CollectListItems();
        };
        
        WeakReferenceMessenger.Default.Register<TaskListIndexViewModel, TaskListSavedMessage>(this, (r, m) =>                                                                                                         
        {                                                                                                                                                                                                        
            repository.Save(m.List);
            r.RefreshLists(); 
        });    
        
        WeakReferenceMessenger.Default.Register<TaskListIndexViewModel, TaskListDeleteMessage>(this, (r, m) =>
        {
            repository.Delete(m.Id);
            r.RefreshLists();                                                                                                                                                                 
        });   
        
        WeakReferenceMessenger.Default.Register<TaskListIndexViewModel, TaskListReorderMessage>(this, (r, m) =>
        {
            if (m.Dragged is not TaskCardViewModel dragged || m.Target is not TaskCardViewModel target) return;

            var oldIndex = r.TaskLists.IndexOf(dragged);
            var newIndex = r.TaskLists.IndexOf(target);
            if (oldIndex < 0 || newIndex < 0 || oldIndex == newIndex) return;
            
            r.TaskLists.Move(oldIndex, newIndex);
        });
        
        WeakReferenceMessenger.Default.Register<TaskListIndexViewModel, TaskListsReorderedMessage>(this, (r, m) =>
        {
            for (var i = 0; i < r.TaskLists.Count; i++)
            {
                r.TaskLists[i].TaskListObject.Order = i;
                repository.Save(r.TaskLists[i].TaskListObject);
            }
        });
    }
}