using System;
using System.Globalization;
using System.Windows.Data;

namespace Launcher
{
    class RankConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val = System.Convert.ToInt32(value);
            if (val > 0) { return val; } else { return ""; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val = System.Convert.ToInt32(value);
            return val;
        }
    }
}
