using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Messages;
using TodoAvalonia.Models;

namespace TodoAvalonia.Data;

public class TaskListStore
{
    public ObservableCollection<TaskList> TaskLists { get; }
    private readonly ITaskListRepository _repository;

    public TaskListStore(ITaskListRepository repository)
    {
        _repository = repository;

        TaskLists = new ObservableCollection<TaskList>(repository.GetAll());

        WeakReferenceMessenger.Default.Register<TaskListStore, TaskListSavedMessage>(this, (s, t) =>
        {
            s._repository.Save(t.List);

            if (s.TaskLists.All(i => i.Id != t.List.Id))
            {
                s.TaskLists.Add(t.List);
            }
        });
        
        WeakReferenceMessenger.Default.Register<TaskListStore, TaskListDeleteMessage>(this, (s, t) =>
        {
            s._repository.Delete(t.Id);

            s.TaskLists.Remove(s.TaskLists.FirstOrDefault(i => i.Id == t.Id)!);
        });
        
        WeakReferenceMessenger.Default.Register<TaskListStore, TaskListsReorderedMessage>(this, (s, t) =>
        {
            for (var i = 0; i < s.TaskLists.Count; i++)
            {
                s.TaskLists[i].Order = i;
            }

            s._repository.SaveAll(s.TaskLists);
        });
    }

    public void Move(TaskList dragged, int newIndex)
    {
        var found = TaskLists.IndexOf(dragged);

        if (found == -1 || found == newIndex) return;
        
        TaskLists.Move(found, newIndex);
    }
}