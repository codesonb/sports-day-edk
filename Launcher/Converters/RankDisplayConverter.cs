using System;
using System.Globalization;
using System.Windows.Data;

using EDKv5;
using EDKv5.Protocols;

namespace Launcher
{
    class RankDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var setting = (ILaneSetting)value;
            if (ResultState.Rank != setting.State)
                return setting.State.ToString();
            else
                return setting.Rank;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
