using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TodoAvalonia.Data;
using TodoAvalonia.Models;
using TodoAvalonia.Stores;

namespace TodoAvalonia.ViewModels;

public static class DesignData
{
    public static TaskListIndexViewModel DesignTaskLists { get; } =
        new(new TaskListStore(new DesignTaskListRepository()));

    public static TaskList DesignTaskList { get; } = new()
    {
        Title = "Task title",
        CreatedAt = DateTime.UtcNow,
        ExpiredAt = DateTime.UtcNow,
        TaskItems =
        [
            new TaskItem { Id = Guid.NewGuid(), Content = "Buy milk" },
            new TaskItem { Id = Guid.NewGuid(), Content = "Buy bread", IsDone = true }
        ]
    };

    public static TaskListViewModel DesignTaskListItem { get; } = new(DesignTaskList);
    public static MainViewModel DesignMain { get; } = new() { MainContent = DesignTaskLists };
    public static TaskListFormViewModel DesignTaskListForm { get; } = new() { ModalTitle = "Modal Title" };

    private class DesignTaskListRepository : ITaskListRepository
    {
        public bool SaveAll(IEnumerable<TaskList> taskLists) => true;
        public List<Guid> ReorderAll(bool descending = false, string? key = null) => [Guid.NewGuid()];

        public bool Create(TaskList taskList) => true;

        public bool Update(TaskList taskList) => true;

        public TaskList? FindById(Guid id) => new TaskList                                                                                                                                                                            
              {                                                                                                                                                                                                    
                  Id = Guid.NewGuid(),                                                                                                                                                                             
                  Title = "Groceries",                                                                                                                                                                             
                  CreatedAt = DateTime.UtcNow,                                                                                                                                                                     
                  TaskItems =                                                                                                                                                                                      
                  [                                                                                                                                                                                                
                      new TaskItem { Id = Guid.NewGuid(), Content = "Buy milk" },                                                                                                                                  
                      new TaskItem { Id = Guid.NewGuid(), Content = "Buy bread", IsDone = true }                                                                                                                   
                  ]                                                                                                                                                                                                
              };

        public bool Delete(Guid id) => true;

        public bool Reorder(Guid id, int newIndex) => true;

        public IEnumerable<TaskList> GetAll(string? search = null)                                                                                                                                               
          {                                                                                                                                                                                                        
              yield return new TaskList                                                                                                                                                                            
              {                                                                                                                                                                                                    
                  Id = Guid.NewGuid(),                                                                                                                                                                             
                  Title = "Groceries",                                                                                                                                                                             
                  CreatedAt = DateTime.UtcNow,                                                                                                                                                                     
                  TaskItems =                                                                                                                                                                                      
                  [                                                                                                                                                                                                
                      new TaskItem { Id = Guid.NewGuid(), Content = "Buy milk" },                                                                                                                                  
                      new TaskItem { Id = Guid.NewGuid(), Content = "Buy bread", IsDone = true }                                                                                                                   
                  ]                                                                                                                                                                                                
              };                                                                                                                                                                                                   
                                                                                                                                                                                                                   
              yield return new TaskList                                                                                                                                                                            
              {                                                                                                                                                                                                    
                  Id = Guid.NewGuid(),                                                                                                                                                                             
                  Title = "Work",                                                                                                                                                                                  
                  CreatedAt = DateTime.UtcNow                                                                                                                                                                      
              };                                                                                                                                                                                                   
                                                                                                                                                                                                                   
              yield return new TaskList                                                                                                                                                                            
              {                                                                                                                                                                                                    
                  Id = Guid.NewGuid(),                                                                                                                                                                             
                  Title = "another reeeealy long title, it's so fucking big...",                                                                                                                                   
                  CreatedAt = DateTime.UtcNow                                                                                                                                                                      
              };                                                                                                                                                                                                   
                                                                                                                                                                                                                   
              yield return new TaskList                                                                                                                                                                            
              {                                                                                                                                                                                                    
                  Id = Guid.NewGuid(),                                                                                                                                                                             
                  Title = "Side project",                                                                                                                                                                          
                  CreatedAt = DateTime.UtcNow                                                                                                                                                                      
              };                                                                                                                                                                                                   
                                                                                                                                                                                                                   
              yield return new TaskList                                                                                                                                                                            
              {                                                                                                                                                                                                    
                  Id = Guid.NewGuid(),                                                                                                                                                                             
                  Title = "Side project",                                                                                                                                                                          
                  CreatedAt = DateTime.UtcNow                                                                                                                                                                      
              };   
              
              yield return new TaskList                                                                                                                                                                            
              {                                                                                                                                                                                                    
                  Id = Guid.NewGuid(),                                                                                                                                                                             
                  Title = "a very long title that takes a lot of space in card, too much letters woooow",                                                                                                          
                  CreatedAt = DateTime.UtcNow                                                                                                                                                                      
              };                                                                                                                                                                                                   
                                                                                                                                                                                                                   
              yield return new TaskList                                                                                                                                                                            
              {                                                                                                                                                                                                    
                  Id = Guid.NewGuid(),                                                                                                                                                                             
                  Title = "Side project",                                                                                                                                                                          
                  CreatedAt = DateTime.UtcNow                                                                                                                                                                      
              };                                                                                                                                                                                                   
          }
    }
}