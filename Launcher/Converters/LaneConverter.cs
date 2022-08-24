using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

using EDKv5;
using System.Windows;

namespace Launcher
{
    class LaneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Project prj = Project.GetInstance();
            int index = prj.LaneOrder[(int)value];
            return Application.Current.FindResource("imgLane" + index);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
            //Project prj = Project.GetInstance();
            //short[] lo = prj.LaneOrder;
            //int lane = (int)value;
            //for (int i = 0; i < lo.Length; i++)
            //    if (lo[i] == lane) return i;
            //return -1;
        }
    }
}
