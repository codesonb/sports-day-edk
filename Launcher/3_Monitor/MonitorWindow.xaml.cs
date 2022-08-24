using EDKv5;
using EDKv5.MonitorServices;
using EDKv5.SchedulerService;
using Launcher.Wrappers;
using Launcher.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Printing;

using System.Windows.Markup;
using System.Xml;

using Launcher.Algorithms;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MonitorWindow.xaml
    /// </summary>
    public partial class MonitorWindow : Window
    {
        private class __CompetitionState
        {
            public __CompetitionState(CompetingGroup competingGroup)
            {
                this.CompetingGroup = competingGroup;
            }
            internal CompetingGroup CompetingGroup { get; }
            internal bool IsCompleted { get; set; }
        }

        #region Initialization
        // constructor
        public MonitorWindow()
        {
            InitializeComponent();

            //-----------------------
            Station = new MonitorStation(Project.GetInstance());

            //-----------------------
            var day = Station.Today;
            schedule.Init(day, true);
            Init_StateMap(day);
            schedule.ItemClicked += Schedule_ItemClicked;

            //-----------------------
            var mediator = MonitorMediator.Instance;
            mediator.CompetitionCompleted += _on_competition_updated;
            mediator.EventCompleted += _on_event_completed;
            mediator.RecordBreaked += _on_record_breaked;
        }

        CompetingGroupVisualAdapter currentVisual;
        private EventCompletionState Schedule_ItemClicked(CompetingGroupVisualAdapter adapter)
        {
            currentVisual = adapter;
            switch (adapter.State)
            {
                case EventCompletionState.Full:
                    // create document
                    var cmps = adapter.CompetingGroup.Event.GetCompetitions(adapter.CompetingGroup.Group);
                    var doc = ResultDocumentFactory.CreateResultDocument(cmps);
                    if (adapter.CompetingGroup.Event.NeedLaneAssignment)
                        doc.SetLayout(PrintLayout.A5);
                    else
                        doc.SetLayout(PrintLayout.A4);

                    // create preview
                    var preview = new PrintDocumentPreviewer();
                    preview.Document = doc;
                    if ( true == preview.ShowDialog() &&
                        _print(doc, adapter.CompetingGroup.Event.NeedLaneAssignment))
                    {
                        return EventCompletionState.Printed;
                    }
                    break;
                case EventCompletionState.WaitRank:
                    var ev = adapter.CompetingGroup.Event;
                    var gp = adapter.CompetingGroup.Group;

                    var cps = ev.GetCompetitions(gp);                                       // all competitions in the group
                    var arr = cps.Where((cmp) => { return !cmp.IsRankMatched; }).ToArray(); // filter un-matched ranks competitions

                    gridInteract.Content = new CompetitionStackCollection(arr);
                    break;
            }
            return adapter.State;
        }

        private void Init_StateMap(IScheduleDay day)
        {
            IPeriod[] periods = day.Periods;
            foreach (IPeriod period in periods)
            {
                ICompetingPeriod icp = period as ICompetingPeriod;
                if (null != icp)
                {
                    foreach (Cell cell in icp.Cells)
                    {
                        var cmpGrp = (CompetingGroup)cell.Data;
                        var ev = cmpGrp.Event;
                        var cmps = ev.GetCompetitions(cmpGrp.Group);

                        foreach (var cmp in cmps)
                            cmpMap.Add(cmp, new __CompetitionState(cmpGrp));
                    }
                }
            }
        }
        #endregion

        // field
        Dictionary<ICompetition, __CompetitionState> cmpMap = new Dictionary<ICompetition, __CompetitionState>();
        MediaElement media;

        // properties
        public MonitorStation Station { get; }

        // observations
        private void _on_competition_updated(object sender, CompetitionStateUpdatedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    // prepare required variables
                    var comp = e.Competition;
                    var compState = cmpMap[comp];
                    var compGrp = compState.CompetingGroup;

                    // check completion
                    compState.IsCompleted = e.Competition.IsCompleted;

                    // show
                    schedule.SetState(compGrp, e.State);

                    //-- update message
                    lvEvents.Items.Insert(0, new EventMessage("C", string.Format(
                        "{0} {1}/R{2} is updated.",
                        comp.Group.GetName(),
                        comp.Event.Name,
                        comp.Index,
                        e.State.ToString()
                    )));

                    // play sound
                    if (EventCompletionState.WaitRank == e.State)
                    {
                        media = (MediaElement)Resources["audioAlert"];
                        media.Stop();
                        media.Play();
                    }

                    // update scoring
                    _update_score();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " at _on_competition_updated() MonitorWindows.xaml.cs");
                }
            }));

        }
        private void _on_event_completed(object sender, EventUpdatedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                //-- update message
                lvEvents.Items.Insert(0, new EventMessage("E", string.Format(
                    "{0} {1} is completed.",
                    e.Group,
                    e.Event.Name
                )));

                // play sound
                media = (MediaElement)this.Resources["audioCoin"];
                media.Stop();
                media.Play();

                //-------
                var log = String.Format(
                    "- EC> [{0}] - {1}",
                    e.Event.ID,
                    e.Event.Name
                );
                Console.WriteLine(log);
            }));


        }
        private void _on_record_breaked(object sender, RecordBreakedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                var log = String.Format(
                    "  RB> [{0}] - {1}, R{2} | [{3}] - {4},{5} {6}",
                    e.Competition.Group.GetName(),
                    e.Competition.Event.Name,
                    e.Competition.Index,
                    e.Participant.ID,
                    e.Participant.House.Name,
                    e.Participant.ClassName,
                    e.Participant.Name
                );
                Console.WriteLine(log);
            }));
        }

        //------------------------------------------------

        private void _update_score()
        {
            var stat = new Algorithms.ScoreStatistic(Project.GetInstance());
            tabStat.DataContext = stat;
        }

        private bool _print(FlowDocument doc, bool isLandscape)
        {
            //Clone the source document
            var cloneDoc = doc.Clone();

            //Now print using PrintDialog
            var dialog = new PrintDialog();
            if (isLandscape)
            {
                var ticket = dialog.PrintTicket;
                ticket.PageOrientation = PageOrientation.Landscape;
                ticket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA5);
            }
            if (dialog.ShowDialog() == true)
            {
                cloneDoc.PageHeight = dialog.PrintableAreaHeight;
                cloneDoc.PageWidth = dialog.PrintableAreaWidth;
                IDocumentPaginatorSource idocument = cloneDoc as IDocumentPaginatorSource;

                dialog.PrintDocument(idocument.DocumentPaginator, "Printing FlowDocument");
                return true;
            }
            return false;
        }

        private void btnPrintHsScore_Click(object sender, RoutedEventArgs e)
        {
            var stat = tabStat.DataContext as ScoreStatistic;
            if (null != stat)
            {
                var doc = ScoreRankDocumentFactory.CreateScoreRankDocument(
                    "House Ranking",
                    Properties.Resources.strHouse,
                    stat.Houses,
                    (hs) => { return hs.Name; }
                );
                doc.SetLayout(PrintLayout.A5);

                var previewer = new PrintDocumentPreviewer();
                previewer.Document = doc;
                if (true == previewer.ShowDialog())
                    _print(doc, true);
            }
        }

        private void btnPrintGrpRank_Click(object sender, RoutedEventArgs e)
        {
            var stat = tabStat.DataContext as ScoreStatistic;
            //if (null != stat)
            //    GroupRanksDocumentFactory.CreateScoreRankDocument(stat.Groups);
        }

        private void btnPrintClsScore_Click(object sender, RoutedEventArgs e)
        {
            var stat = tabStat.DataContext as ScoreStatistic;
            if (null != stat)
            {
                var doc = ScoreRankDocumentFactory.CreateScoreRankDocument(
                    "Class Rankings",
                    Properties.Resources.strHouse,
                    stat.Classes,
                    (cls) => { return cls.Key; }
                );
                doc.SetLayout(PrintLayout.A4);

                var previewer = new PrintDocumentPreviewer();
                previewer.Document = doc;
                if (true == previewer.ShowDialog())
                    _print(doc, false);
            }
        }

        private void btnPrintIndvScore_Click(object sender, RoutedEventArgs e)
        {
            var stat = tabStat.DataContext as ScoreStatistic;
            if (null != stat)
            {
                var indvRanks = new List<Score<Student>>();
                foreach (var scoreStu in stat.Individuals)
                {
                    if (scoreStu.Value.Total <= 0)
                    {
                        MessageBox.Show("Please print the score after accomplishment of more competitions.");
                        return;
                    }
                    if (scoreStu.Rank > 20) break;
                    indvRanks.Add(scoreStu);
                }

                var doc = ScoreRankDocumentFactory.CreateScoreRankDocument(
                    "Individual Rankings Top 20",
                    Properties.Resources.strWinner,
                    indvRanks.ToArray(),
                    (indv) => { return string.Format("[{0}]{1} {2}", indv.House.Key, indv.ClassName, indv.Name); }
                );
                doc.SetLayout(PrintLayout.A4);

                var previewer = new PrintDocumentPreviewer();
                previewer.Document = doc;
                if (true == previewer.ShowDialog())
                    _print(doc, false);
            }
        }
    }
}
