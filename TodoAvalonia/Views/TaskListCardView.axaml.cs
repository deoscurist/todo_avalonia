using Avalonia.Controls;
using Avalonia.Input;
using TodoAvalonia.ViewModels;

namespace TodoAvalonia.Views;

public partial class TaskListCardView : UserControl
{
    public TaskListCardView()
    {
        InitializeComponent();
    }
        
    private void Border_Tapped(object? sender, TappedEventArgs e)
    {
        (DataContext as TaskListViewModel)?.OpenCommand.Execute(null);
    }

    private void MenuButton_Tapped(object? sender, TappedEventArgs e) => e.Handled = true;
}