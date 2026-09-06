using LiteDB;

namespace TodoAvalonia.Models;

public class AppSetting
{
    [BsonId]
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}