using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TodoAvalonia.ViewModels;

namespace TodoAvalonia.Services;

public partial class NotificationService : ObservableObject
{
    public ObservableCollection<NotificationViewModel> Notifications { get; } = new();

    public void Show(string title, string? message = null, string type = "info", int durationMs = 3000)
    {
        var notification = new NotificationViewModel(title, message, type);
        Notifications.Add(notification);

        if (durationMs > 0)
        {
            _ = Task.Delay(durationMs).ContinueWith(_ =>
                Dispatcher.UIThread.Post(() => Notifications.Remove(notification)));
        }
    }

    [RelayCommand]
    private void Dismiss(NotificationViewModel notification)
    {
        Notifications.Remove(notification);
    }
}