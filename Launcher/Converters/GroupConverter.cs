using EDKv5;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Launcher
{
    class GroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Group grp = (Group)value;
            return grp.GetName();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
