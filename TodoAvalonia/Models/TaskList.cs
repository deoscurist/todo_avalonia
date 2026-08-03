using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TodoAvalonia.Models;

public class TaskList
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string? Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ExpiredAt { get; set; }

    public ObservableCollection<TaskItem> TaskItems { get; set; } = [];
} 