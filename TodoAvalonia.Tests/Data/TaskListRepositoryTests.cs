using System;
using System.IO;
using LiteDB;
using TodoAvalonia.Data;
using TodoAvalonia.Models;

namespace TodoAvalonia.Tests.Data;

public class TaskListRepositoryTests : IDisposable
{
    private readonly string _dbPath;
    private readonly LiteDatabase _database;
    private readonly TaskListRepository _repository;

    public TaskListRepositoryTests()
    {
        _dbPath = Path.GetTempFileName();
        _database = new LiteDatabase(_dbPath) { UtcDate = true };
        _repository = new TaskListRepository(_database);
    }

    [Fact]
    public void Save_NewTaskList_CanBeFoundById()
    {
        // Arrange
        var taskList = new TaskList
        {
            Id = Guid.NewGuid(),
            Title = "Groceries",
            CreatedAt = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc),
            TaskItems =
            [
                new TaskItem { Id = Guid.NewGuid(), Content = "Buy milk", IsDone = false },
                new TaskItem { Id = Guid.NewGuid(), Content = "Buy bread", IsDone = true }
            ]
        };

        // Act
        _repository.Save(taskList);
        var retrieved = _repository.Get(taskList.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(taskList.Id, retrieved.Id);
        Assert.Equal(taskList.Title, retrieved.Title);
        Assert.Equal(taskList.CreatedAt, retrieved.CreatedAt);
        Assert.Equal(taskList.TaskItems.Count, retrieved.TaskItems.Count);
        Assert.Equal(taskList.TaskItems[0].Content, retrieved.TaskItems[0].Content);
        Assert.Equal(taskList.TaskItems[0].IsDone, retrieved.TaskItems[0].IsDone);
        Assert.Equal(taskList.TaskItems[1].Content, retrieved.TaskItems[1].Content);
        Assert.Equal(taskList.TaskItems[1].IsDone, retrieved.TaskItems[1].IsDone);
    }

    public void Dispose()
    {
        _database.Dispose();
        File.Delete(_dbPath);
    }
}
