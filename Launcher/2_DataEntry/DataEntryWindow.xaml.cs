using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;

using EDKv5;
using EDKv5.Protocols;
using EDKv5.MonitorServices;
using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace Launcher
{
    public partial class DataEntryWindow : Window
    {
        #region Initialization
        public DataEntryWindow()
        {
            InitializeComponent();
            //--------
            colvw_id = new CollectionViewSource();
            colvw_id.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));
            colvw_lane = new CollectionViewSource();
            colvw_lane.SortDescriptions.Add(new SortDescription("Lane", ListSortDirection.Ascending));
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (null == Station) { throw new Exception("Connection is not set."); }

            var mediator = ResponseMediator.Instance;
            mediator.GotEventOutlines += update_EventOutlines;
            mediator.GotCompetitionDetail += update_CompetitionDetail;

            Station.RequestOutline();
        }
        #endregion

        // fields
        #region Fields
        LaneSettingAdapter[] adapters;      // ~ retain original order
        CollectionViewSource colvw_id;
        CollectionViewSource colvw_lane;
        CompetitionResponse curComp;        // current processing competition
        #endregion

        // properties
        #region Property
        public DataEntryStation Station { get; set; }
        #endregion

        #region Operations
        private void _send_compDetail_request(int id)
        {
            //----
            if (null != curComp)
            {
                var results = new List<CompetitionResult>();
                foreach (var item in adapters)
                    results.Add(item.ResultObject);
                Station.RequestUpdateResults(curComp.CompetitionID, results.ToArray());
            }
            //----

            Station.RequestCompetition(id);
            expandCompList.IsExpanded = false;
        }
        #endregion

        //~~~~~~~~~~~~~~~~~~~~~~~~~~
        // UI and event notifications
        #region Observer Update
        private void update_EventOutlines(EventOutlinesResponse resp)
        {
            // Cross-Thread invocation
            Dispatcher.Invoke(new Action(() =>
            {
                colvw_id.Source = resp.EventOutlines;
                tvcomps.ItemsSource = colvw_id.View;
            }));
        }
        private void update_CompetitionDetail(CompetitionResponse resp)
        {
            // Cross-Thread invocation
            Dispatcher.Invoke(new Action(() =>
            {
                //-----
                grid.DataContext = curComp = resp;
                LaneSettingAdapter.ResultType = resp.ResultType;

                //----- udpate the result entering UI
                adapters = new LaneSettingAdapter[resp.Lanes.Length];
                for (int i = 0; i < adapters.Length; i++)
                    adapters[i] = new LaneSettingAdapter(resp.Lanes[i]);

                colvw_lane.Source = adapters;
                lvAthletList.ItemsSource = colvw_lane.View;

                //------ update the competition ID
                txtCompID.Text = curComp.CompetitionID.ToString();
            }));
        }
        #endregion

        #region UI - Click Event Handler
        private void selectCompetition_Click(object sender, RoutedEventArgs e)
        {
            var s = (FrameworkElement)sender;
            var id = (int)s.DataContext;
            _send_compDetail_request(id);
        }
        #endregion

        #region UI - Result Textbox Operation
        private void txtResult_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textbox = (TextBox)sender;
            var data = (LaneSettingAdapter)textbox.DataContext;
            bool suppress = data.IsSuppressInput(textbox.Text, e.Text[0]);
            e.Handled = suppress;
        }
        private void txtResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = (TextBox)sender;
            var data = (LaneSettingAdapter)textbox.DataContext;
            data.Result = textbox.Text;
        }

        private void txtResult_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox thisBox;
            ListViewItem item;

            switch (e.Key)
            {
                case Key.Down:
                case Key.Up:
                    thisBox = (TextBox)sender;
                    item = thisBox.ClosestParent<ListViewItem>();
                    DependencyObject anchor;
                    if (Key.Down == e.Key)
                    { anchor = item.Next(); }
                    else
                    { anchor = item.Prev(); }

                    if (null != anchor)
                    {
                        var nextBox = anchor.First<TextBox>();
                        nextBox.Focus();
                    }
                    break;
                case Key.Right:
                    thisBox = (TextBox)sender;
                    item = thisBox.ClosestParent<ListViewItem>();
                    var thisRankBox = item.nth<TextBox>(2);
                    thisRankBox.Focus();
                    break;
            }
        }
        #endregion

        #region UI - Rank Textbox Operation
        private void txtRank_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.D0 <= e.Key && Key.D9 >= e.Key)
                return;
            if (Key.NumPad0 <= e.Key && Key.NumPad9 >= e.Key)
                return;

            TextBox thisBox;
            ListViewItem item;
            switch (e.Key)
            {
                case Key.Back:
                case Key.Delete:
                case Key.Home:
                case Key.End:
                case Key.Right:
                    return;
                case Key.Left:
                    thisBox = (TextBox)sender;
                    item = thisBox.ClosestParent<ListViewItem>();
                    var thisResultBox = item.First<TextBox>();
                    thisResultBox.Focus();
                    break;
                case Key.Up:
                case Key.Down:
                    thisBox = (TextBox)sender;
                    item = thisBox.ClosestParent<ListViewItem>();
                    DependencyObject anchor;

                    if (Key.Down == e.Key)
                    { anchor = item.Next(); }
                    else
                    { anchor = item.Prev(); }

                    if (null != anchor)
                    {
                        var nextBox = anchor.nth<TextBox>(2);
                        nextBox.Focus();
                    }
                    break;
            }
            e.Handled = true;
        }

        private void txtRank_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = (TextBox)sender;
            var data = (LaneSettingAdapter)textbox.DataContext;
            short val = 0;
            if (textbox.Text.Length > 0)
                val = Convert.ToInt16(textbox.Text);
            data.Rank = val;
        }
        #endregion

        #region UI - ID Textbox
        private void txtCompID_KeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Enter == e.Key && txtCompID.Text.Length > 0)
            {
                int id;
                if (int.TryParse(txtCompID.Text, out id))
                { _send_compDetail_request(id); }
                else
                {
                    // show error
                    infoBox.Visibility = Visibility.Visible;
                    infoBox.BeginStoryboard((Storyboard)infoBox.Resources["animate"]);
                }
            }
        }
        #endregion

    }
}
