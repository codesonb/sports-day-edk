using System;
using System.Collections.ObjectModel;
using EDKv5;
using System.Windows;
using System.Collections.Generic;

namespace Launcher
{
    public class EventType : DependencyObject
    {
        public static DependencyProperty ChildrenProperty = DependencyProperty.Register("Children",
            typeof(ObservableCollection<EDKv5.Event>), typeof(EventType), new PropertyMetadata(null));

        public int ID { get; set; }
        public string Name { get; set; }
        public ObservableCollection<EDKv5.Event> Items { get; }
        public EventType(int id, string name, ObservableCollection<EDKv5.Event> items) { ID = id; Name = name; Items = items; }
    }
}
