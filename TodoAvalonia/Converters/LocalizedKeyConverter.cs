using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using TodoAvalonia.Langs;

namespace TodoAvalonia.Converters;

public class LocalizedKeyConverter : IMultiValueConverter
{
    public static readonly LocalizedKeyConverter Instance = new();                                                                                                                                               
                                                                                                                                                                                                                   
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)                                                                                                       
        => values.Count > 0 && values[0] is string key ? Localizer.Instance[key] : null; 
}