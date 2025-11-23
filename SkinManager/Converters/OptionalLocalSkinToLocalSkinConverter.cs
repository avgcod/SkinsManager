using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using LanguageExt;
using SkinManager.Types;

namespace SkinManager.Converters;

public class OptionalLocalSkinToLocalSkinConverter : IValueConverter{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Option<LocalSkin> optionalSkin){
            return optionalSkin.Match(currentSkin => 
                    !string.IsNullOrWhiteSpace(currentSkin.Author)
                        ? $"{currentSkin.SkinName} by {currentSkin.Author}"
                        : currentSkin.SkinName
                ,
                () => "None");
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Option<LocalSkin>.None;
    }
    
}