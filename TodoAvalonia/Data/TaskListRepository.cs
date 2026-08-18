using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using TodoAvalonia.Models;

namespace TodoAvalonia.Data;

public class TaskListRepository(LiteDatabase db) : ITaskListRepository
{
    private readonly LiteDatabase _db = db;
    private readonly ILiteCollection<TaskList> _taskLists = db.GetCollection<TaskList>("tasklist");
    
    public bool Save(TaskList taskList)
    {
        var isNew = !_taskLists.Exists(t => t.Id == taskList.Id);                                                                                                                                                    
        if (isNew)                                                                                                                                                                                                   
        {                                                                                                                                                                                                            
            var maxOrder = _taskLists.FindAll().Select(t => (int?)t.Order).Max() ?? -1;                                                                                                                              
            taskList.Order = maxOrder + 1;                                                                                                                                                                           
        } 
        
        _taskLists.Upsert(taskList);
            
        return true;
    }
    
    public bool Delete(Guid id)
    {
        _taskLists.Delete(id);
        
        return true;
    }

    public TaskList? Get(Guid id)
    {
        return _taskLists.FindById(id);
    }

    public IEnumerable<TaskList> GetAll(string? search = null)
    {
        IEnumerable<TaskList> results; 
        
        if (!string.IsNullOrEmpty(search))
        {
            results = _taskLists.Find(taskList =>
                taskList.Title != null && (taskList.Title.Contains(search) ||
                                           taskList.TaskItems.Any(item => item.Content != null && item.Content.Contains(search))));
        }
        else
        {
            results = _taskLists.FindAll();
        }

        return results.OrderBy(taskList => taskList.Order);
    }
}