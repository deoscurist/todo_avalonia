using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Messages;
using TodoAvalonia.ViewModels;

namespace TodoAvalonia.Views;

public partial class TaskListAddButtonView : UserControl
{
    public TaskListAddButtonView()
    {
        InitializeComponent();
    }
    
    public void OnClick(object? sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ShowModalMessage(new TaskListFormViewModel()));
    }         
}