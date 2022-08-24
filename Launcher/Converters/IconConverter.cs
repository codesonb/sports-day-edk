using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

using EDKv5;
using System.Windows;

namespace Launcher
{
    class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int eid = System.Convert.ToInt32(value);
            if (eid == -3)
                return Application.Current.FindResource("imgTrack");
            else if (eid == -2)
                return Application.Current.FindResource("imgField");
            else if (eid == -1)
                return Application.Current.FindResource("imgSwim");
            else if (eid <= (int)EventIndex.SportsDayEnd)
            {
                return ((ResourceDictionary)Application.Current.FindResource("imgIcons"))[eid.ToString()];
            }
            else if (eid <= (int)EventIndex.SwimGalaEnd)
                return Application.Current.FindResource("imgSwim");
            else
                return Application.Current.FindResource("imgRelay");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
