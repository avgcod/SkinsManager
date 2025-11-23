using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace SkinManager.Converters;

public class EmptyAuthorToUnknown: IValueConverter{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch{
            string currentAuthor => !string.IsNullOrWhiteSpace(currentAuthor) ? currentAuthor : "Unknown",
            _ => AvaloniaProperty.UnsetValue
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}