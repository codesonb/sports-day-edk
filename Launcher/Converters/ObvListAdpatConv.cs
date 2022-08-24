using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

using EDKv5;
using System.Windows;
using System.Collections.ObjectModel;

namespace Launcher
{
    class ObservableListAdpaterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type orOrTyp = value.GetType();
            Type[] orGnArg;
            if (orOrTyp.HasElementType)
                orGnArg = new Type[] { orOrTyp.GetElementType() };
            else
                orGnArg = orOrTyp.GetGenericArguments();
            Type tgTyp = typeof(ObservableCollectionAdapter<>).MakeGenericType(orGnArg);
            var constructor = tgTyp.GetConstructors()[0];
            var instance = constructor.Invoke(new object[] { value });

            return instance;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
