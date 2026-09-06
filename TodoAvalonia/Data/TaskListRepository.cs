using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using TodoAvalonia.Data.DB;
using TodoAvalonia.Langs;
using TodoAvalonia.Models;

namespace TodoAvalonia.Data;

public class TaskListRepository(TasksDatabase context) : ITaskListRepository
{
    private readonly LiteDatabase _db = context.Database;
    private readonly ILiteCollection<TaskList> _taskLists = context.Database.GetCollection<TaskList>("tasklist");

    public TaskList? FindById(Guid id) => _taskLists.FindById(id);

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

    public bool Create(TaskList taskList)
    {
        try
        {
            taskList.Order = _taskLists.Count();

            _taskLists.Insert(taskList);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _db.Rollback();
            throw new Exception(Localizer.Instance["Error.CreateFailed"], e);
        }

        return true;
    }

    public bool Update(TaskList taskList)
    {
        var stored = _taskLists.FindById(taskList.Id);

        if (stored is null) return false;

        try
        {
            taskList.Order = stored.Order;

            return _taskLists.Update(taskList);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _db.Rollback();
            throw new Exception(Localizer.Instance["Error.UpdateFailed"], e);
        }
    }

    public bool Delete(Guid id)
    {
        var stored = _taskLists.FindById(id);

        if (stored is null) return false;

        var orderStart = stored.Order;

        _db.BeginTrans();

        try
        {
            _taskLists.Delete(id);

            _taskLists.UpdateMany(
                t => new TaskList { Order = t.Order - 1 },
                t => t.Order > orderStart);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _db.Rollback();
            throw new Exception(Localizer.Instance["Error.DeleteFailed"], e);
        }

        _db.Commit();

        return true;
    }

    public bool Reorder(Guid id, int newIndex)
    {
        var stored = _taskLists.FindById(id);

        if (stored is null) return false;
        if (newIndex < 0 || newIndex >= _taskLists.Count()) return false;

        var oldIndex = stored.Order;

        if (oldIndex == newIndex) return true;

        _db.BeginTrans();

        try
        {
            if (oldIndex < newIndex)
            {
                _taskLists.UpdateMany(
                    t => new TaskList { Order = t.Order - 1 },
                    t => t.Order > oldIndex && t.Order <= newIndex);
            }
            else
            {
                _taskLists.UpdateMany(
                    t => new TaskList { Order = t.Order + 1 },
                    t => t.Order >= newIndex && t.Order < oldIndex);
            }

            _taskLists.UpdateMany(
                t => new TaskList { Order = newIndex },
                t => t.Id == id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _db.Rollback();
            throw new Exception(Localizer.Instance["Error.ReorderFailed"], e);
        }

        _db.Commit();

        return true;
    }

    public List<Guid> ReorderAll(bool descending = false, string? key = null)
    {
        _db.BeginTrans();
        
        List<Guid> listId;
        
        try
        {
            var items = _taskLists.FindAll();

            var ordered = (key switch
            {
                "created" => descending
                    ? items.OrderByDescending(taskList => taskList.CreatedAt)
                    : items.OrderBy(taskList => taskList.CreatedAt),

                "expired" => descending
                    ? items.OrderByDescending(taskList => taskList.ExpiredAt ?? DateTime.MaxValue)
                    : items.OrderBy(taskList => taskList.ExpiredAt ?? DateTime.MaxValue),

                "status" => descending
                    ? items.OrderByDescending(taskList =>
                        taskList.TaskItems.Count > 0 && taskList.TaskItems.All(item => item.IsDone) ? 2
                        : taskList.ExpiredAt < DateTime.UtcNow ? 0 : 1)
                    : items.OrderBy(taskList =>
                        taskList.TaskItems.Count > 0 && taskList.TaskItems.All(item => item.IsDone) ? 2
                        : taskList.ExpiredAt < DateTime.UtcNow ? 0 : 1),

                _ => items.OrderBy(taskList => taskList.Order)
            }).ToList();
            
            listId = ordered.Select(taskList => taskList.Id).ToList();

            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].Order = i;
            }

            _taskLists.Update(ordered);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _db.Rollback();
            throw new Exception(Localizer.Instance["Error.ReorderFailed"], e);
        }

        _db.Commit();

        return listId;
    }

    public bool SaveAll(IEnumerable<TaskList> taskLists)
    {
        _db.BeginTrans();

        try
        {
            _taskLists.Update(taskLists);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _db.Rollback();
            throw new Exception(Localizer.Instance["Error.SaveAllFailed"], e);
        }

        _db.Commit();

        return true;
    }
}
