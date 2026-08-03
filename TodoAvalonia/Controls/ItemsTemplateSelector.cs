using Avalonia.Controls;
using Avalonia.Controls.Templates;
using TodoAvalonia.Models;
using TodoAvalonia.ViewModels;
using TodoAvalonia.Views;

namespace TodoAvalonia.Controls;

public class ItemsTemplateSelector : IDataTemplate
{
    public Control? Build(object? param)
    {
        return param switch
        {
            TaskList => new TaskCardView(),
            CreateTaskViewModel => new CreateTaskView(),
            _ => null
        };
    }

    public bool Match(object? data)
    {
        return data is TaskList or CreateTaskViewModel;
    }
}