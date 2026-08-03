using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Data;
using TodoAvalonia.Messages;
using TodoAvalonia.Models;

namespace TodoAvalonia.ViewModels;

public class TaskListIndexViewModel : ViewModelBase
{
    public ObservableCollection<TaskList> TaskLists { get; set; } = new();
    public ObservableCollection<object> ListItems { get; } = new();
    
    private void CollectListItems()
    {
        ListItems.Clear();
        ListItems.Add(new CreateTaskViewModel());

        foreach (var taskList in TaskLists)
            ListItems.Add(taskList);
    }

    public TaskListIndexViewModel(ITaskListRepository repository)
    {
        foreach (var taskList in repository.GetAll())                                                                                                                                                            
            TaskLists.Add(taskList); 
        
        CollectListItems();
        TaskLists.CollectionChanged += (_, _) => CollectListItems();
        
        WeakReferenceMessenger.Default.Register<TaskListIndexViewModel, TaskListSavedMessage>(this, (r, m) =>                                                                                                         
        {                                                                                                                                                                                                        
            repository.Save(m.List);                                                                                                                                                                             
            r.TaskLists.Clear();                                                                                                                                                                                 
                                                                                                                                                                                                                   
            foreach (var taskList in repository.GetAll())                                                                                                                                                        
                r.TaskLists.Add(taskList);                                                                                                                                                                       
        });    
    }
}