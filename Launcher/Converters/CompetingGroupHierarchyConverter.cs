using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

using EDKv5;
using EDKv5.Protocols;
using System.Collections.ObjectModel;

namespace Launcher
{
    class CompetingGroupHierarchyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dict = new Dictionary<Group, IList<CompetitionOutline>>();
            CompetitionOutline[] outlines = (CompetitionOutline[])value;

            foreach (CompetitionOutline outline in outlines)
            {
                IList<CompetitionOutline> x;
                if (!dict.TryGetValue(outline.CompGroup, out x))
                    dict.Add(outline.CompGroup, x = new ObservableCollection<CompetitionOutline>());

                x.Add(outline);
            }

            return dict;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
