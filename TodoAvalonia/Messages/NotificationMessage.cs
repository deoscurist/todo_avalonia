namespace TodoAvalonia.Messages;

public record NotificationMessage(string Title, string? Message = null, string Type = "info");