using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Data;
using TodoAvalonia.Messages;

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
        
        TaskLists.CollectionChanged += (_, _) => CollectListItems();
        
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
    }
}