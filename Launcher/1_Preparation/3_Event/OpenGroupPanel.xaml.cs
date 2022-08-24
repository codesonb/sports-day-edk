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
    public partial class OpenGroupPanel : UserControl, IPageSwitch, IPage
    {
        public event EventHandler CallPreviousPage;
        public event EventHandler CallNextPage;

        //constructor
        #region Initialization
        public OpenGroupPanel()
        {
            InitializeComponent();

            //-----
            for (int i = 0; i < grps.Length; i++)
                grps[i] = new ObservableCollection<Group>();
        }
        private void gridAssigner_Loaded(object sender, RoutedEventArgs e)
        {

            Border[] grpItms = new Border[]
            {
                itemGrpBx0, itemGrpBx1, itemGrpBx2,
                itemGrpBx3, itemGrpBx4, itemGrpBx5,
                itemGrpBx6, itemGrpBx7, itemGrpBx8
            };

            for (int i = 0; i < grpItms.Length; i++)
            {
                var content = (ContentControl)grpItms[i].Child;
                ListBox ls = content.First<ListBox>();
                ls.ItemsSource = grps[i];
            }

        }
        public void Init()
        {
            Project prj = Project.GetInstance();
            Event[] evs = prj.Events;
            tvOpen.ItemsSource = evs;
        }

        #endregion

        // fields
        #region Fields
        IList<Group>[] grps = new IList<Group>[9];
        #endregion

        #region Main Operation

        #endregion

        #region Main UI Mouse Drag and Double Clicks

        #endregion

        //-----
        // Side Buttons
        private void btnIndPreset_Click(object sender, RoutedEventArgs e)
        {
            Event ev = tvOpen.SelectedItem as Event;
            if (null == ev) return;
            ev.CloseAllGroups();
            ev.OpenGroup(Group.MA);
            ev.OpenGroup(Group.MB);
            ev.OpenGroup(Group.MC);
            ev.OpenGroup(Group.FA);
            ev.OpenGroup(Group.FB);
            ev.OpenGroup(Group.FC);
            _update_display_group(ev);
        }
        private void btnGenPreset_Click(object sender, RoutedEventArgs e)
        {
            Event ev = tvOpen.SelectedItem as Event;
            if (null == ev) return;
            ev.CloseAllGroups();
            ev.OpenGroup(Group.Male & ~Group.MD);
            ev.OpenGroup(Group.Female & ~Group.FD);
            _update_display_group(ev);
        }
        private void btnPubPreset_Click(object sender, RoutedEventArgs e)
        {
            Event ev = tvOpen.SelectedItem as Event;
            if (null == ev) return;
            ev.CloseAllGroups();
            ev.OpenGroup(Group.A | Group.B | Group.C);
            _update_display_group(ev);
        }

        //----------
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            this.CallPreviousPage?.Invoke(this, e);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            this.CallNextPage?.Invoke(this, e);
        }

        private void itemGrp_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                FrameworkElement s = (FrameworkElement)sender;
                object context = s.DataContext;
                DragDrop.DoDragDrop(s, context, DragDropEffects.Move);
            }
        }
        private void itemGrp_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Group)))
            {
                // reference prepare
                ListBox ls = (ListBox)e.Source;
                IList<Group> bindRef = (IList<Group>)ls.ItemsSource;

                // remove
                Group grp = (Group)e.Data.GetData(typeof(Group));
                for (int i = 0; i < grps.Length; i++)
                {
                    if (grps[i].Contains(grp))
                    {
                        if (bindRef == grps[i]) { return; } // stop if moving to same reference
                        grps[i].Remove(grp); break;
                    }
                }
                // add to binded display
                bindRef.Add(grp);

                // update backend
                _update_grps_to_event((Event)tvOpen.SelectedItem);
            }
        }


        //-----------------------------
        // main operation
        private void _update_display_group(Event ev)
        {
            Group notOp = Group.None;
            if (null == ev)
            {
                for (int i = 1; i < grps.Length; i++)
                    grps[i].Clear();
            }
            else
            {
                Group[] _grp = ev.OpenedGroups;
                int i = 0;
                for (; i < _grp.Length; i++)
                {
                    grps[i + 1].Clear();
                    Group tg = (Group)0x80u;
                    while (tg > 0)
                    {
                        Group g = tg & _grp[i];
                        if (g > 0)
                        {
                            grps[i + 1].Add(g);
                        }
                        tg = (Group)((uint)tg >> 1);
                    }
                    notOp |= _grp[i];
                }
                for (++i; i < grps.Length; i++)
                    grps[i].Clear();
            }

            //---------------- that not opened
            grps[0].Clear();
            notOp = ~notOp;
            Group tng = (Group)0x80u;
            while (tng > 0)
            {
                Group g = tng & notOp;
                if (g > 0) { grps[0].Add(g); }
                tng = (Group)((uint)tng >> 1);
            }
        }
        private void _update_grps_to_event(Event ev)
        {
            ev.CloseAllGroups();
            for (int i = 1; i < grps.Length; i++) // first item is NOT open group
            {
                if (grps[i].Count > 0)
                {
                    Group g = Group.None;
                    foreach (Group _g in grps[i]) { g |= _g; }
                    ev.OpenGroup(g);
                }
            }
        }

        private void tvOpen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lsBx = (ListBox)sender;
            Event ev = (Event)lsBx.SelectedItem;
            _update_display_group(ev);
        }

        #region IPage Implementation
        UIElement IPage.UI { get { return this; } }
        void IPage.In() { Init(); }
        void IPage.BeforeOut() { }
        #endregion


    }
}
