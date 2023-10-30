using Avalonia.Collections;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SkinManager.Converters
{
    public class DiameterAndThicknessToStrokeDashArrayConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2 ||
                !double.TryParse(values[0]?.ToString(), out double diameter) ||
                !double.TryParse(values[1]?.ToString(), out double thickness))
            {
                return 0;
            }

            double circumference = Math.PI * diameter;

            double lineLength = circumference * 0.75;
            double gapLength = circumference - lineLength;

            return new AvaloniaList<double>(new[] { lineLength / thickness, gapLength / thickness });
        }
    }
}
