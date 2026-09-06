using System;
using System.Collections.Generic;
using TodoAvalonia.Models;

namespace TodoAvalonia.Data;

public interface ITaskListRepository
{
    bool SaveAll(IEnumerable<TaskList> taskLists);
    List<Guid> ReorderAll(bool descending = false, string? key = null);
    bool Create(TaskList taskList);
    bool Update(TaskList taskList);
    public TaskList? FindById(Guid id);
    bool Delete(Guid id);
    bool Reorder(Guid id, int newIndex);
    IEnumerable<TaskList> GetAll(string? search = null);
}