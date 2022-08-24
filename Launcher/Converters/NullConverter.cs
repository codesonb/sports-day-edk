using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

using EDKv5;
using System.Windows;

namespace Launcher
{
    class NullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
