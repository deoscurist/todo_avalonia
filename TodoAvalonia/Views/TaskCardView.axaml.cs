using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Messages;

namespace TodoAvalonia.Views;

public partial class TaskCardView : UserControl
{
    public TaskCardView()
    {
        InitializeComponent();
    }
}