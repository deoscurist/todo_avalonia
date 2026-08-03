using System;

namespace TodoAvalonia.Models;

public class TaskItem
{
    public required Guid Id { get; init; }
    public required string Content { get; set; }
    public bool IsDone { get; set; }
}