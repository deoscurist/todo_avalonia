using CommunityToolkit.Mvvm.ComponentModel;

namespace TodoAvalonia.ViewModels;

public partial class NotificationViewModel : ObservableObject
{
    public string Title { get; }
    public string? Message { get; } = null;
    public string Type { get; }
    public bool HasMessage => !string.IsNullOrEmpty(Message);
    
    public NotificationViewModel(string title, string? message = null, string type = "info")
    {
        Title = title;
        Message = message;
        Type = type;
    }
}