using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TodoAvalonia.Converters;

public class TaskListStatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value as string) switch
        {
            "Status.Incomplete" => "Blue",
            "Status.Outdated" => "Red",
            "Status.Completed" => "LimeGreen",
            _ => "Black"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value as string) switch
        {
            "Orange" => "Status.Incomplete",
            "OrangeRed" => "Status.Outdated",
            "LimeGreen" => "Status.Completed",
            "Black" => null
        };
    }
}