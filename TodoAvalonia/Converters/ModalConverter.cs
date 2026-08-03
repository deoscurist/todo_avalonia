using Avalonia.Data.Converters;

namespace TodoAvalonia.Converters;

public static class ModalConverter
{
    public static readonly IValueConverter NullToOpacity =                                                                                                                                                       
        new FuncValueConverter<object?, double>(content => content is not null ? 1 : 0); 
}