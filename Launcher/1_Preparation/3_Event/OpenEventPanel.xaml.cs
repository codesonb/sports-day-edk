using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using EDKv5;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for StudentInfoPanel.xaml
    /// </summary>

    public partial class OpenEventPanel : UserControl, IPageSwitch
    {
        public event EventHandler CallPreviousPage;
        public event EventHandler CallNextPage;

        //constructor
        #region Initialization
        public OpenEventPanel()
        {
            InitializeComponent();

            Project prj = Project.GetInstance();
            Event[] evs = prj.Events;

            //start importing values
            string[] evNames = EDKv5.Events.GetNames();

            opened = new ObservableCollection<Event>();
            preset = new ObservableCollection<EventType>();

            preset.Add(new EventType(-3, Properties.Resources.strTrackEvents, tls = new ObservableCollection<Event>()));
            preset.Add(new EventType(-2, Properties.Resources.strFieldEvents, fls = new ObservableCollection<Event>()));
            preset.Add(new EventType(-1, Properties.Resources.strSwimGala, sls = new ObservableCollection<Event>()));

            Action<int, int, ObservableCollection<Event>, Func<int, Event>> _do_assignment = (a, b, p, c) =>
            {
                for (int i = a; i <= b; i++)
                {
                    Event ev = null;

                    //stupid code, Max run @ 32^2 = 1024 // can use hash table instead
                    foreach (Event e in evs)
                        if (Convert.ToInt32(e.ID) == i) { ev = e; break; }

                    if (ev != null) {
                        opened.Add(ev);
                    } else {
                        ev = c(i);
                        p.Add(ev);
                    }
                }
            };

            // create track
            _do_assignment((int)EventIndex.TrackEventStart, (int)EventIndex.TrackEventEnd, tls, (i) =>
            {
                bool assignLane = i < (int)EventIndex.Track800 || i > (int)EventIndex.Track1500;
                bool useLongTime = i >= (int)EventIndex.Track400 && i <= (int)EventIndex.Track1500;
                return Event.Create(i, evNames[i], assignLane, useLongTime);
            });
            // create field
            _do_assignment((int)EventIndex.FieldEventStart, (int)EventIndex.FieldEventEnd, fls, (i) =>
            {
                return Event.Create(i, evNames[i]);
            });
            // create swim
            _do_assignment((int)EventIndex.SwimGalaStart, (int)EventIndex.SwimGalaEnd, sls, (i) =>
            {
                bool useLongTime = i > (int)EventIndex.Medley100;
                return Event.Create(i, evNames[i], true, useLongTime);
            });

            //set custom
            foreach (Event e in evs)
            {
                if (Convert.ToInt32(e.ID) >= (int)EventIndex.Custom)
                    opened.Add(e);
            }

            //-----
            CollectionViewSource sortedPreset = new CollectionViewSource();
            sortedPreset.Source = preset;
            sortedPreset.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));
            tvPreset.ItemsSource = sortedPreset.View;

            CollectionViewSource sortedOpened = new CollectionViewSource();
            sortedOpened.Source = opened;
            sortedOpened.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));
            tvOpen.ItemsSource = sortedOpened.View;

        }
        #endregion

        // fields
        #region Fields
        ObservableCollection<Event> opened;
        ObservableCollection<EventType> preset;
        ObservableCollection<Event> tls;
        ObservableCollection<Event> fls;
        ObservableCollection<Event> sls;
        #endregion

        #region Main Operation
        private void _open_events(Event[] evs)
        {
            Project prj = Project.GetInstance();
            foreach (Event ev in evs)
            {
                // remove
                if (ev.IsSwim)
                {
                    sls.Remove(ev);
                }
                else if (ev.IsField)
                {
                    fls.Remove(ev);
                }
                else if (Convert.ToInt32(ev.ID) <= (int)EventIndex.TrackEventEnd)
                {
                    tls.Remove(ev);
                }
                // move
                opened.Add(ev);
                prj.AddEvent(ev);
            }
        }
        private void _revert_event(Event ev)
        {
            // remove
            Project prj = Project.GetInstance();
            opened.Remove(ev);
            prj.RemoveEvent(ev);

            // move
            if (ev.IsSwim)
            {
                sls.Add(ev);
            }
            else if (ev.IsField)
            {
                fls.Add(ev);
            }
            else if (Convert.ToInt32(ev.ID) <= (int)EventIndex.TrackEventEnd)
            {
                tls.Add(ev);
            }
            // else, just remove

        }
        #endregion

        #region Main UI Mouse Drag and Double Clicks
        // drag drops
        private void tvPreset_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                FrameworkElement s = (FrameworkElement)e.Source;
                object context = s.DataContext;

                if (context is EventType)
                {
                    EventType data = (EventType)context;
                    DragDrop.DoDragDrop(tvPreset, data.Items.ToArray(), DragDropEffects.Move);
                }
                else if (context is Event)
                {
                    Event[] evs = new Event[] { (Event)context };
                    DragDrop.DoDragDrop(tvPreset, evs, DragDropEffects.Move);
                }
            }
        }
        private void tvOpen_DragEnter(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == tvPreset)
                e.Effects = DragDropEffects.Move;
            else
                e.Effects = DragDropEffects.None;
        }
        private void tvOpen_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Event[])))
            {
                // move all sub-items
                Event[] evs = (Event[])e.Data.GetData(typeof(Event[]));
                _open_events(evs);
            }
        }
        // double clicks
        private void itemParent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement s = (FrameworkElement)e.Source;
            object context = s.DataContext;
            if (context is EventType)
            {
                EventType data = (EventType)context;
                _open_events(data.Items.ToArray());
            }
            else if (context is Event)
            {
                Event[] evs = new Event[] { (Event)context };
                _open_events(evs);
            }
        }
        private void itemRevert_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement s = (FrameworkElement)e.Source;
            object context = s.DataContext;

            Event ev = (Event)context;
            _revert_event(ev);
        }
        #endregion

        //-----
        // Side Buttons
        #region Side Buttons (Right)
        private void btnNewCustom_Click(object sender, RoutedEventArgs e)
        {
            Project prj = Project.GetInstance();

            int k = (int)EventIndex.Custom - 1;
            while (++k < 100)
            {
                Event @event;
                if (!prj.TryGetEvent(k.ToString("00"), out @event)) break;
            }

            //set limit
            if (k >= 100)
            {
                MessageBox.Show("Sorry, you have reached the limit.");
                return;
            }
            Event ev = Event.Create(
                k, "Custom Event " + (k - (int)EventIndex.Custom + 1).ToString(),
                true, true
            );
            opened.Add(ev);
            prj.AddEvent(ev);
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (null != tvPreset.SelectedItem)
            {
                object data = tvPreset.SelectedItem;
                if (data is EventType)
                {
                    Event[] evs = ((EventType)data).Items.ToArray();
                    _open_events(evs);
                } else if (data is Event) {
                    Event[] evs = new Event[] { (Event)data };
                    _open_events(evs);
                }
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (null != tvOpen.SelectedItem)
            {
                Event ev = (Event)tvOpen.SelectedItem;
                _revert_event(ev);
            }
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            foreach (Event ev in opened.ToArray())
                _revert_event(ev);
        }

        #endregion

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            this.CallNextPage?.Invoke(this, e);
        }
    }
}
