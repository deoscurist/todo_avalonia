using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TodoAvalonia.ViewModels;

namespace TodoAvalonia.Views;

public partial class MainFooterView : UserControl
{
    public MainFooterView()
    {
        InitializeComponent();

        DataContext = new MainFooterViewModel(App.AppSettingStore);
    }
}