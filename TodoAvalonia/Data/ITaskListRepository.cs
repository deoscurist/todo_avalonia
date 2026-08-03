using System;
using System.Collections.Generic;
using TodoAvalonia.Models;

namespace TodoAvalonia.Data;

public interface ITaskListRepository
{
    bool Save(TaskList taskList);
    bool Delete(Guid id);
    TaskList? Get(Guid id);
    IEnumerable<TaskList> GetAll(string? search = null);
}