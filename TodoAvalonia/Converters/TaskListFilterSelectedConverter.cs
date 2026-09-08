using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TodoAvalonia.Converters;

public class TaskListFilterSelectedConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.Equals(value as string, parameter as string, StringComparison.Ordinal);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}