namespace TodoAvalonia.Messages;

public record TaskListReorderMessage(object Dragged, int NewIndex);