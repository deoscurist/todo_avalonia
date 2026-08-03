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
        if (!string.IsNullOrEmpty(search))
        {
            return _taskLists.Find(taskList =>
                taskList.Title.Contains(search) ||
                taskList.TaskItems.Any(item => item.Content.Contains(search)));
        }

        return _taskLists.FindAll();
    }
}