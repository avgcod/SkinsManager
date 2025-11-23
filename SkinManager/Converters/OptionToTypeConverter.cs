using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using LanguageExt;
using SkinManager.Types;

namespace SkinManager.Converters;

public class OptionToTypeConverter : IValueConverter{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue; 
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture){
        return value switch{
            DisplaySkin currentSkin => currentSkin,
            _ => Option<DisplaySkin>.None
        };
        /*if (value is DisplaySkin currentSkin){
            return currentSkin switch{
                WebSkin theWebSkin => Option.Some<Skin>(theWebSkin),
                LocalSkin theLocalSkin => Option.Some<Skin>(theLocalSkin),
                _ => Option<Skin>.None
            };
        }

        return Option<Skin>.None;*/
    }
    
}