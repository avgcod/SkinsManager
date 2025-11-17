using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using SkinManager.Types;

namespace SkinManager.Converters;

public class SkinSourceToStringConverter : IValueConverter{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is WebSkin theWebSkin){
            string author = string.IsNullOrWhiteSpace(theWebSkin.Author) ? "Unknown Author" : theWebSkin.Author;
            return $"{Enum.GetName<SkinsSource>(theWebSkin.Source)} {theWebSkin.SkinName} by {author}.";
        }
        else if (value is LocalSkin theLocalSkin)
        {
            string author = string.IsNullOrWhiteSpace(theLocalSkin.Author) ? "Unknown Author" : theLocalSkin.Author;
            return $"Local {theLocalSkin.SkinName} by {author}.";
        }
        else{
            return AvaloniaProperty.UnsetValue;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        /*if (value is string text && text.Split(' ') is { Length: 3 } strings && parameter is IEnumerable<Skin> skins){
            return skins.First(currentSkin => currentSkin switch{
                WebSkin currentWebSkin => currentWebSkin.SkinName == strings[1],
                LocalSkin currentLocalSkin => currentLocalSkin.SkinName == strings[1],
                _ => throw new NotSupportedException()
            });
        }*/

        return AvaloniaProperty.UnsetValue;
    }
}