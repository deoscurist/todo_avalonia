using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Data;
using TodoAvalonia.Messages;
using TodoAvalonia.Models;

namespace TodoAvalonia.Stores;

public class TaskListStore
{
    private readonly ITaskListRepository _repository;
    
    private readonly ObservableCollection<TaskList> _taskLists;
    public ReadOnlyObservableCollection<TaskList> TaskLists { get; }

    public TaskListStore(ITaskListRepository repository)
    {
        _repository = repository;
        
        _taskLists = new ObservableCollection<TaskList>(repository.GetAll());
        TaskLists = new ReadOnlyObservableCollection<TaskList>(_taskLists);
    }

    public void Create(TaskList taskList)
    {
        var created = _repository.Create(taskList);
        
        if (created) _taskLists.Add(taskList);
    }

    public void Update(TaskList list)
    {
        _repository.Update(list);
    }

    public void Delete(TaskList list)
    {
        var deleted = _repository.Delete(list.Id);

        if (deleted) _taskLists.Remove(list);
    }

    public void ReorderAll(bool descending = false, string? key = null)
    {
        try
        {
            var listId = _repository.ReorderAll(descending, key);

            var target = 0;

            for (var i = 0; i < listId.Count; i++)
            {
                var list = _taskLists.FirstOrDefault(t => t.Id == listId[i]);

                if (list is null) continue;

                var current = _taskLists.IndexOf(list);

                if (current != target) _taskLists.Move(current, target);

                target++;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public void Reorder(TaskList dragged, int newIndex)
    {
        var found = _taskLists.IndexOf(dragged);

        if (found == -1 || found == newIndex) return;
        
        var moved = _repository.Reorder(dragged.Id, newIndex);
        
        if (moved) _taskLists.Move(found, newIndex);
    }
}