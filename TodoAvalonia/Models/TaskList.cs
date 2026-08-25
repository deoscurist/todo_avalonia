using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TodoAvalonia.Models;

public partial class TaskList : ObservableObject
{
    public Guid Id { get; init; } = Guid.NewGuid();
    [ObservableProperty] public partial string? Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    [NotifyPropertyChangedFor(nameof(Status))] [ObservableProperty] public partial DateTime? ExpiredAt { get; set; }
    public int Order { get; set; }

    [NotifyPropertyChangedFor(nameof(Status))] [ObservableProperty] public partial ObservableCollection<TaskItem> TaskItems { get; set; } = [];

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