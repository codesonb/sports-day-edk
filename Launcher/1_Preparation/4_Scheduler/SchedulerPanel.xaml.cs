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
using Xceed.Wpf.Toolkit;
using EDKv5.SchedulerService;
using System.Collections.ObjectModel;
using System.ComponentModel;

using IO = System.IO;
using Microsoft.Win32;

namespace Launcher
{

    public partial class SchedulerPanel : UserControl
    {
        public SchedulerPanel()
        {
            InitializeComponent();
            //----
            unassignedSorter = new CollectionViewSource();
            unassignedSorter.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));
            //lsCmpGrp.ItemsSource = unassignedSorter.View;
        }

        //field
        #region Fields
        CompetingGroup[] _ls_preview;
        CollectionViewSource unassignedSorter;
        #endregion


        #region Update UI
        internal void Update_CompetingGroupList(Schedule schedule)
        {
            unassignedSorter.Source = schedule.UnassignedCompetitions;
            lsCmpGrp.ItemsSource = unassignedSorter.View;

            schTab.Items.Clear();
            IScheduleDay[] days = schedule.Days;
            for (int i = 0; i < days.Length; i++)
            {
                TabItem tbItm = new TabItem();
                tbItm.Header = "Day " + (i + 1);
                schTab.Items.Add(tbItm);

                var schV = new SchedulerView();
                schV.EditMode = true;               // enable drop events to modify schedule
                schV.Init(schedule.Days[i], true);
                tbItm.Content = schV;
            }
            schedule.Updated += _schedule_onUpdated;
        }
        private void _schedule_onUpdated(object sender, EventArgs e)
        {
            unassignedSorter.Source = ((Schedule)sender).UnassignedCompetitions;
            lsCmpGrp.ItemsSource = unassignedSorter.View;
        }
        #endregion

        #region Drag Drop UI Operation
        private void previewJot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var list = lsCmpGrp.SelectedItems;
            _ls_preview = new CompetingGroup[list.Count];
            list.CopyTo(_ls_preview, 0);
        }
        private void startDragCmpGrp_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseButtonState.Pressed == e.LeftButton)
            {
                if (null != _ls_preview && _ls_preview.Length > 0)
                {
                    DragDrop.DoDragDrop(this, _ls_preview, DragDropEffects.Move);
                }
            }
        }
        #endregion

        private void btnSaveImg_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = Properties.Resources.filterExtImagesAll;

            if (true == sfd.ShowDialog())
            {
                var x = (TabItem)schTab.SelectedItem;
                var schView = (UIElement)x.Content;

                ImageFormat[] formats = new ImageFormat[]
                {
                    ImageFormat.JPEG,
                    ImageFormat.PNG,
                    ImageFormat.BMP,
                    ImageFormat.GIF,
                    ImageFormat.TIFF
                };

                using (IO.Stream stream = sfd.OpenFile())
                {
                    schView.ScreenShot(formats[sfd.FilterIndex - 1], stream); // filter index start at 1
                }
            }
        }
    }
}
