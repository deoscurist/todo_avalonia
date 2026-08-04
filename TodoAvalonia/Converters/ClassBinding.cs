using System;
using Avalonia;
using Avalonia.Controls;

namespace TodoAvalonia.Converters;

public class ClassBinding
{
    // Это наше собственное привязываемое свойство для классов
    public static readonly AttachedProperty<string?> ClassesProperty =
        AvaloniaProperty.RegisterAttached<Control, string?>("Classes", typeof(ClassBinding));

    static ClassBinding()
    {
        ClassesProperty.Changed.AddClassHandler<Control>((control, e) =>
        {
            var newValue = e.NewValue as string;
            // Очищаем все старые классы
            control.Classes.Clear();
            if (!string.IsNullOrEmpty(newValue))
            {
                // Разбиваем строку по пробелам и добавляем каждый класс
                foreach (var cls in newValue.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    control.Classes.Add(cls);
                }
            }
        });
    }

    public static string? GetClasses(AvaloniaObject element) => element.GetValue(ClassesProperty);
    public static void SetClasses(AvaloniaObject element, string? value) => element.SetValue(ClassesProperty, value);
}