using Avalonia.Data.Converters;
using SkinManager.Types;
using System;
using System.Globalization;

namespace SkinManager.Converters
{
    public class SelectedSourceEnumToTestConverter : IValueConverter
    {
        private string _lastSource = string.Empty;
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is null)
            {
                return string.Empty;
            }
            else
            {
                _lastSource = ((SkinsSource)value).ToString();
                return _lastSource;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is { } comboBoxItem)
            {
                _lastSource = (comboBoxItem as string)!;
            }
            
            return Enum.Parse<SkinsSource>(_lastSource!);
        }
    }
}
