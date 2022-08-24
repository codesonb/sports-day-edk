using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Launcher
{
    class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Drawing.Color color;
            if (value is int)
                color = System.Drawing.Color.FromArgb((int)value);
            else
                color = (System.Drawing.Color)value;
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (Color)value;
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
