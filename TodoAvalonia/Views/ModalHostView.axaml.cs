using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Messages;

namespace TodoAvalonia.Views;

public partial class ModalHostView : UserControl
{
    public ModalHostView()
    {
        InitializeComponent();
    }
    
    private void Close_Click(object? sender, RoutedEventArgs e) =>                                                                                                                                                   
        WeakReferenceMessenger.Default.Send(new CloseModalMessage());  
}