using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Launcher
{
    class ColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (System.Drawing.Color)value;
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = ((SolidColorBrush)value).Color;
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
