using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using SkinManager.Extensions;
using SkinManager.Types;
using System.Linq;

namespace SkinManager.Converters;

public class SkinsToDisplaySkinsConverter  : IValueConverter{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture){
        if (value is IEnumerable<Skin> skins){
            return skins.Aggregate(new List<DisplaySkin>(), (convertedSkins, currentSkin) =>
                currentSkin switch{
                    WebSkin theWebSkin => [..convertedSkins, theWebSkin.ToDisplaySkin()],
                    LocalSkin theLocalSkin => [..convertedSkins, theLocalSkin.ToDisplaySkin()]
                });
        }

        return AvaloniaProperty.UnsetValue;
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}