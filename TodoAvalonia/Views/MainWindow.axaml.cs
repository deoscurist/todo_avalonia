using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using TodoAvalonia.ViewModels;

namespace TodoAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)                                                                                                                                  
    {                                                                                                                                                                                                                
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)                                                                                                                                                  
            BeginMoveDrag(e);                                                                                                                                                                                        
    }                                                                                                                                                                                                                

    private void Minimize_Click(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;                                                                                                           
    private void Maximize_Click(object? sender, RoutedEventArgs e) =>                                                                                                                                                
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;                                                                                                             
    private void Close_Click(object? sender, RoutedEventArgs e) => Close();  
}