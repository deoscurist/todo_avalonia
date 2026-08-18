namespace TodoAvalonia.Messages;

public record TaskListReorderMessage(object Dragged, object Target);