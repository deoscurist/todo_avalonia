using System;

namespace TodoAvalonia.Models;

public class TaskItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string? Content { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}