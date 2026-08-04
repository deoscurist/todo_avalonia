using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Messages;
using TodoAvalonia.ViewModels;

namespace TodoAvalonia.Views;

public partial class CreateTaskView : UserControl
{
    public CreateTaskView()
    {
        InitializeComponent();
    }

    private void Border_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as CreateTaskViewModel)?.OpenCommand.Execute(null);
    }                                                                                                                             
}