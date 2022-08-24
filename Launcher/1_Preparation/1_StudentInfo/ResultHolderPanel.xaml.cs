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
using System.Threading.Tasks;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for ResultHolderPanel.xaml
    /// </summary>
    public partial class ResultHolderPanel : UserControl
    {
        public ResultHolderPanel()
        {
            InitializeComponent();

            //
            string[] evNames = EDKv5.Events.GetNames();

            //
            preset = new ObservableCollection<EventType>();
            preset.Add(new EventType(-3, Properties.Resources.strTrackEvents, tls = new ObservableCollection<Event>()));
            preset.Add(new EventType(-2, Properties.Resources.strFieldEvents, fls = new ObservableCollection<Event>()));
            preset.Add(new EventType(-1, Properties.Resources.strSwimGala, sls = new ObservableCollection<Event>()));

            Action<int, int, ObservableCollection<Event>, Func<int, Event>> _do_assignment = (a, b, p, c) =>
            {
                for (int i = a; i <= b; i++) { p.Add(c(i)); }
            };

            // create track
            _do_assignment((int)EventIndex.TrackEventStart, (int)EventIndex.TrackEventEnd, tls, (i) =>
            {
                return Event.Create(i, evNames[i]);
            });
            // create field
            _do_assignment((int)EventIndex.FieldEventStart, (int)EventIndex.FieldEventEnd, fls, (i) =>
            {
                return Event.Create(i, evNames[i]);
            });
            // create swim
            _do_assignment((int)EventIndex.SwimGalaStart, (int)EventIndex.SwimGalaEnd, sls, (i) =>
            {
                return Event.Create(i, evNames[i]);
            });

            //-----
            CollectionViewSource sortedPreset = new CollectionViewSource();
            sortedPreset.Source = preset;
            sortedPreset.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));
            tvEvents.ItemsSource = sortedPreset.View;

        }

        // fields
        ObservableCollection<EventType> preset;
        ObservableCollection<Event> tls;
        ObservableCollection<Event> fls;
        ObservableCollection<Event> sls;

        EventIndex _curEvent;
        CompetitionResult _curResult;
        ICompetitionResultType _curResultType;

        // ~
        private void showMessage(string msg) { lbMsg.Content = msg; }
        private void tvEvents_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Event d = e.NewValue as Event;

            if (null != d)
            {
                Project project = Project.GetInstance();

                this._curEvent = (EventIndex)Enum.Parse(typeof(EventIndex), d.ID);
                this._curResultType = Event.Create((int)_curEvent, "").ResultType;

                lbEventName.Content = Events.GetDefaultNames()[(int)this._curEvent];
                lvCurHolder.ItemsSource = project.GetHolders(this._curEvent);
                createTempResultObject();
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (null == lvCurHolder.SelectedItem) { return; }

            var tmpTuple = (Tuple<Group, HolderRecord>)lvCurHolder.SelectedItem;
            Project project = Project.GetInstance();
            project.RemoveHolder(tmpTuple.Item2.Event, tmpTuple.Item2.Group);
            lvCurHolder.ItemsSource = project.GetHolders(this._curEvent);

        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // get result
            if (ResultState.Rank != _curResult.State || 0u == _curResult.Value)
            {
                lbMsg.Content = "Please enter a valid result value.";
                return;
            }

            // get name
            string name = txtHolder.Text;
            if (name.Length <= 0)
            {
                lbMsg.Content = "Please enter the holder's name.";
                return;
            }

            // get year
            int year = datePicker.SelectedDate.Value.Year;

            // get groups
            Group grp = Group.None;
            if (chkMA.IsChecked.Value) grp |= Group.MA;
            if (chkMB.IsChecked.Value) grp |= Group.MB;
            if (chkMC.IsChecked.Value) grp |= Group.MC;
            if (chkMD.IsChecked.Value) grp |= Group.MD;
            if (chkFA.IsChecked.Value) grp |= Group.FA;
            if (chkFB.IsChecked.Value) grp |= Group.FB;
            if (chkFC.IsChecked.Value) grp |= Group.FC;
            if (chkFD.IsChecked.Value) grp |= Group.FD;
            if (Group.None == grp)
            {
                lbMsg.Content = "Please select groups.";
                return;
            }

            // create holder
            var holder = new HolderRecord(_curEvent, grp, name, year, _curResult);

            // apply new holder
            Project project = Project.GetInstance();
            project.SetHolder(this._curEvent, grp, holder);

            // create new holder object
            createTempResultObject();
            lvCurHolder.ItemsSource = project.GetHolders(this._curEvent);
        }

        private void createTempResultObject()
        {
            if (null == _curResultType) { return; }

            _curResult = _curResultType.CreateResult();
            // clear current inputs
            txtHolder.Text = "";
            txtResult.Text = "";
        }

        private void txtResult_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textbox = (TextBox)sender;
            if (null == _curResultType)
            {
                textbox.Text = "";
                lbMsg.Content = "Please select an event.";
                e.Handled = true;
                return;
            }

            char newChar = e.Text[0];
            switch (newChar)
            {
                case '*':
                case '+':
                case '-':
                    e.Handled = true; // suppress the non-ranking input
                    break;
                default:
                    e.Handled = _curResult.IsSuppressInput(textbox.Text, newChar);
                    break;
            }
        }
        private void txtResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null == _curResult) { return; }

            var textbox = (TextBox)sender;
            _curResult.Result = textbox.Text;
        }

    }
}
