using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TodoAvalonia.Models;

public class TaskList
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string? Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ExpiredAt { get; set; }
    public int Order { get; set; }

    public ObservableCollection<TaskItem> TaskItems { get; set; } = [];

    public string Status
    {
        get
        {
            var hasIncompleteItems = TaskItems.All(item => !item.IsDone);

            if (hasIncompleteItems)
            {
                return ExpiredAt < DateTime.Now ? "outdated" : "incomplete";
            }
            
            return "completed";
        }
    }
} 