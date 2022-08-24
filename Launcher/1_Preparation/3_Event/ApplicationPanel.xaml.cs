using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using EDKv5;
using EDKv5.Statistics;
using EDKv5.Utility.Scanners;
using EDKv5.Utility.PrintDocuments;

using Launcher.Scanners;

using IO = System.IO;
using D = System.Drawing;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for StudentInfoPanel.xaml
    /// </summary>

    public partial class ApplicationPanel : UserControl, IPageSwitch, IPage
    {
        public event EventHandler ScannedAll;
        public event EventHandler CallPreviousPage;
        public event EventHandler CallNextPage;

        private class ScanAdapter
        {
            public EntryImageScanner Scanner
            {
                get
                {
                    if (null == _scanner) _scanner = new EntryImageScanner(_scanOutputAdapter);
                    return _scanner;
                }
            }
            public int SuccessAttempt { get; private set; }
            public int FailedAttempt { get; private set; }
            public int ErrorCount { get { return Scanner.ErrorCount; } }
            public Tuple<Student, Event>[] FailJoin { get { return _scanOutputAdapter.FailJoin; } }

            EntryImageScanner _scanner;
            EntryScanOutputAdapter _scanOutputAdapter = new EntryScanOutputAdapter();

            public void Init(Project project)
            {
                _scanOutputAdapter.Init(project);
                Scanner.ErrorCount = 0;
                SuccessAttempt = 0;
                FailedAttempt = 0;
            }
            public void Scan(D.Image img)
            {
                try
                {
                    if (!Scanner.Scan(img, SuccessAttempt + FailedAttempt))
                        throw new Exception("Cannot recognize the image");
                    SuccessAttempt++;
                }
                catch (Exception ex)
                {
                    FailedAttempt++;
                }
            }
        }

        //constructor
        #region Initialization
        public ApplicationPanel()
        {
            InitializeComponent();
            this.ScannedAll += _after_scan;
        }
        #endregion

        // fields
        #region Fields
        ScanAdapter scanAdapter = new ScanAdapter();

        bool isStudentReady = false;
        bool isEventReady = false;
        int errCount = 0;

        #endregion

        #region Main Operation
        private void scanEntryForms(string[] paths)
        {
            // update UI
            ldOverlay.Visibility = Visibility.Visible;

            // call action start on new thread
            Task task = new Task(() => { _do_scanEntryForms(paths); });
            task.Start();

        }
        private void _after_scan(object sender, EventArgs e)
        {
            // --
            Project prj = Project.GetInstance();

            // auto assign lanes
            Event[] evs = prj.Events;
            foreach (Event ev in evs)
                ev.AssignLanes();

            // cross-thread invocation
            this.Dispatcher.Invoke(new Action(() =>
            {
                // update UI
                lbTtl.Content = (scanAdapter.SuccessAttempt + scanAdapter.FailedAttempt).ToString();
                lbSuc.Content = scanAdapter.SuccessAttempt.ToString();
                lbFail.Content = scanAdapter.FailedAttempt.ToString();
                lbFPC.Content = scanAdapter.ErrorCount.ToString();

                ParticipationStatistic statistic = EDKv5.Events.ParticipationStatistic;
                lbPpt.Content = statistic.Total.ToString();

                lvErlog.ItemsSource = scanAdapter.FailJoin;

                // Update UI
                ldOverlay.Visibility = Visibility.Hidden;
            }));

        }
        private void _do_scanEntryForms(string[] paths)
        {
            //init scanner
            Project project = Project.GetInstance();
            scanAdapter.Init(project);
            //scanAdapter.Scanner.CellSample = trkSample.Value;
            //scanAdapter.Scanner.Compensation = trkCompensation.Value;

            //loop through files
            foreach (string path in paths)
            {
                switch (IO.Path.GetExtension(path).ToLower())
                {
                    case @".bmp":
                    case @".jpg":
                    case @".jpeg":
                    case @".png":
                        D.Image img = null;
                        try
                        {
                            img = D.Image.FromFile(path);
                            scanAdapter.Scan(img);
                        }
                        catch (Exception ex)
                        {
                            Console.Write("[img] Read Application Form Failed: ");
                            Console.WriteLine(ex.Message);
                        }
                        finally
                        {
                            //destory image - release memory
                            if (null != img)
                            {
                                img.Dispose();
                                GC.Collect(GC.GetGeneration(img), GCCollectionMode.Forced);
                                img = null;
                            }
                        }
                        break;
                }
            } // end for each path(file)

            //completion callback
            if (null != this.ScannedAll)
                this.ScannedAll(this, EventArgs.Empty);
        }
        #endregion

        #region Main UI Mouse Drag
        private void scanner_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                List<string> ops = new List<string>();
                foreach (string path in paths)
                {
                    string ext = IO.Path.GetExtension(path).ToLower();
                    switch (ext)
                    {
                        case @".jpg":
                        case @".jpeg":
                        case @".png":
                        case @".bmp":
                            break;          // need all to be image
                        default:
                            return;
                    }
                }
                e.Effects = DragDropEffects.All;
            }
        }
        private void scanner_DropEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                List<string> ops = new List<string>();
                foreach (string path in paths)
                {
                    string ext = IO.Path.GetExtension(path).ToLower();
                    switch (ext)
                    {
                        case @".jpg":
                        case @".jpeg":
                        case @".png":
                        case @".bmp":
                            ops.Add(path);
                            break;
                    }
                }

                if (ops.Count > 0)
                {
                    scanEntryForms(ops.ToArray());
                }
            }
        }
        #endregion

        #region UI Button
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (null == cmbPrinters.SelectedItem) return;

            Project prj = Project.GetInstance();

            //check students ready
            if (!isStudentReady)
            {
                MessageBox.Show("Please import students data first");
                return;
            }

            //check events ready
            if (errCount > 0)
            {
                MessageBoxResult dr = MessageBox.Show(
                    string.Format(Properties.Resources.msgEventGroup1, errCount),
                    Properties.Resources.strWarning,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (dr == MessageBoxResult.Yes)
                {
                    //clone copy iteration - safe
                    foreach (Event ev in prj.Events)
                    {
                        if (ev.OpenedGroups.Length <= 0)
                            prj.RemoveEvent(ev);
                    }

                    //update UI
                    this.RefreshReadyLights();
                }
                else
                {
                    return;
                }
            }

            //set confirm
            prj.IsEventConfirmed = true;

            //get events
            Event[] evs = prj.Events;
            PrintDocument form =
#if TEST
                new Debug.EntryForm(prj);
#else
                new EntryForm(prj);
#endif
            form.PrinterSettings.PrintFileName = "[EDK] Student Entry Forms";
            form.PrinterSettings.PrinterName = (string)cmbPrinters.SelectedItem;

            form.Print();
        }
        #endregion

        #region UI Updates
        private void RefreshReadyLights()
        {
            //check state
            Project prj = Project.GetInstance();

            //students
            if (isStudentReady = (prj.StudentsCount > 0))
                idcStuInfo.Fill = Brushes.LawnGreen;
            else
                idcStuInfo.Fill = Brushes.Red;

            //events
            if (!(isEventReady = prj.IsEventConfirmed))
            {
                List<string> err = new List<string>();

                //counter
                foreach (Event ev in prj.Events)
                {
                    if (ev.OpenedGroups.Length <= 0)
                        err.Add(ev.Name);
                }

                errCount = err.Count;

                //set
                if (!(isEventReady = (err.Count <= 0)))
                {
                    Console.WriteLine("Update display hint - not implemented");
                    // StringBuilder sb = new StringBuilder(Properties.Resources.msgEventGroup2 + "\n");
                    // foreach (string e in err)
                    //    sb.AppendLine(e);

                    // lbEv.Text = sb.ToString();
                }
            }

            if (isEventReady)
                idcEvReady.Fill = Brushes.LawnGreen;
            else
                idcEvReady.Fill = Brushes.Red;

            //-----
            btnPrint.IsEnabled = isStudentReady;
        }
        #endregion
        //-----

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            this.CallNextPage?.Invoke(this, e);
        }
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            Project prj = Project.GetInstance();
            if (prj.IsEventConfirmed)
            {
                MessageBoxResult result = MessageBox.Show("If you want to modify the events setting, all schedule and participation records must be deleted.", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                if (MessageBoxResult.OK != result)
                    return;
                else
                {
                    // remove the schedule
                    prj.Schedule = null;
                    foreach (Event ev in prj.Events)
                        ev.ClearParticipants();
                    prj.IsEventConfirmed = false;
                }
            }
            this.CallPreviousPage?.Invoke(this, e);
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "Application Form Image|*.jpg;*.jpeg;*.png;*.bmp";
            ofd.Multiselect = true;

            bool? rs = ofd.ShowDialog();
            if (true == rs)
            {
                scanEntryForms(ofd.FileNames);
            }
        }

        #region Interface Implementation
        UIElement IPage.UI { get { return this; } }
        void IPage.In() { this.RefreshReadyLights(); }
        void IPage.BeforeOut() { }
        #endregion


    }
}
