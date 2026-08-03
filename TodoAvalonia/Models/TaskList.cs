using System;
using System.Collections.Generic;

namespace TodoAvalonia.Models;

public class TaskList
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ExpiredAt { get; set; }

    public List<TaskItem> TaskItems { get; set; } = [];
} 