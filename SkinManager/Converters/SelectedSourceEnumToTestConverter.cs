using Avalonia.Controls;
using Avalonia.Data.Converters;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinManager.Converters
{
    public class SelectedSourceEnumToTestConverter : IValueConverter
    {
        private string lastSource = string.Empty;
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is null)
            {
                return string.Empty;
            }
            if (value is { } enumValue)
            {
                lastSource = ((SkinsSource)enumValue).ToString();
                return ((SkinsSource)enumValue).ToString();
            }
            else
            {
                return lastSource;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is { } comboBoxItem)
            {
                lastSource = ((ComboBoxItem)comboBoxItem)?.Content as string ?? string.Empty;
                return Enum.Parse<SkinsSource>(lastSource);
            }
            else
            {
                return Enum.Parse<SkinsSource>(lastSource);
            }
        }
    }
}
