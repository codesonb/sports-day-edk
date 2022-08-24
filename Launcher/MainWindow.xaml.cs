using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using EDKv5;
using EDKv5.MonitorServices;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        const string C_SAVE_PATH = @"save";
        const string C_CONFIG_PATH = @"config/eventnames.config.txt";

        const int C_PORT = 9423;

        public MainWindow()
        {
            InitializeComponent();

            //=================================================================================
            // initialized global required information
            CompetingGroup.GetNameMethod = (c) => { return c.Group.GetName() + " - " + c.Event.Name; };
            //=================================================================================

            //init functional controls
            bool b = btnLoad.IsEnabled = btnLast.IsEnabled = Directory.Exists(C_SAVE_PATH);
            if (!b)
            {
                //create save folder
                Directory.CreateDirectory(C_SAVE_PATH);
            }
            else
            {
                //check save folder
                DateTime lastProjectDate = DateTime.Now;

                //loop through files
                DirectoryInfo dir = new DirectoryInfo(C_SAVE_PATH);
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo f in files)
                {
                    if (Properties.Resources.extProject == f.Extension)
                    {
                        try
                        {
                            Project tmpPrj = Project.Load(f.FullName);
                            // ---- read success ----

                            //get create time
                            string createTimeStr = System.IO.Path.GetFileNameWithoutExtension(f.Name);

                            try
                            {
                                DateTime createDate = tmpPrj.CreationDate;
                                createTimeStr = createDate.ToString("g");

                                //update most wanted last project
                                if (createDate < lastProjectDate)
                                {
                                    lastProjectDate = createDate;
                                    lastProject = tmpPrj;
                                }
                            }
                            catch
                            {
                                createTimeStr = Properties.Resources.strUnknown;
                            }

                            //get project name
                            string prjName = tmpPrj.Name;
                            string modTimeStr = tmpPrj.LastModify.ToString("g");

                            lvPreviousProjects.Items.Add(tmpPrj);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("-------------------------------------");
                            Console.WriteLine("#PRE-LOAD ERR");
                            Console.WriteLine(ex.GetType().Name);
                            Console.WriteLine(ex.StackTrace);
                        }
                        finally { /* on error resume next */ }
                    }
                } // end for each file in @save

                btnLast.IsEnabled = lastProject != null;
                if (!btnLast.IsEnabled)
                {
                    btnLast.Foreground = Brushes.Gray;
                }

            } // end if dir exist

        }

        // fields
        Grid __gridMenu;
        Project lastProject;
        DataEntryStation station;

        Grid GridMenu
        {
            get { return __gridMenu; }
            set
            {
                if (null == __gridMenu)
                {
                    __gridMenu = value;
                    __gridMenu.Visibility = Visibility.Visible;
                    blurEff.FrameworkElement = __gridMenu;
                }
            }
        }

        //==========================================================================================================
        #region Title Bar Operations
        private void _dragMoveThis(object sender, MouseEventArgs e) { this.DragMove(); }
        private void btnClose_Click(object sender, RoutedEventArgs e) { this.Close(); }
        private void btnMinimize_Click(object sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }
        #endregion

        //==========================================================================================================
        // 1st Level Menu
        #region 1st Level Menu
        private void btnNew_Click(object sender, RoutedEventArgs e) { GridMenu = gridCreate; }
        private void btnLast_Click(object sender, RoutedEventArgs e)
        {
            Project.Set(lastProject);
            PerperationWindow pw = new PerperationWindow();
            pw.Show();
            App.Current.MainWindow = pw;
            this.Close();
        }
        private void btnLoad_Click(object sender, RoutedEventArgs e) { GridMenu = gridLoad; }
        private void btnMonitor_Click(object sender, RoutedEventArgs e)
        {
            // TODO: -- start monitor
            Project.Set(lastProject);
            MonitorWindow monitor = new MonitorWindow();
            monitor.Station.Start();
            monitor.Show();
            App.Current.MainWindow = monitor;
            this.Close();
        }
        private void btnDataEntry_Click(object sender, RoutedEventArgs e)
        {
            // TODO: -- start monitor (debug) & connect to it
            GridMenu = gridConnect;

#if DEBUG
            Project.Set(lastProject);
            MonitorWindow monitor = new MonitorWindow();
            monitor.Station.Start();
            monitor.Show();
#endif

            Task.Factory.StartNew(() =>
            {
                try
                {
                    station = new DataEntryStation();
                    if (station.SeekServer())
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            btnCloseMenu_Click(sender, e);          // ensure shader is shutdown before launch form disposal
                            var window = new DataEntryWindow();
                            window.Station = station;
                            window.Show();
                            App.Current.MainWindow = window;
                            this.Close();
                        }));
                    }
                    else
                    {
                        throw new Exception("Unknow error");
                    }
                }
                catch (TimeoutException ex)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        btnCloseMenu_Click(sender, e);
                        MessageBox.Show("Cannot find server. Please make sure that you have started the ");
                    }));
                }
            });

        }
        #endregion

        //==========================================================================================================
        // Action
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            btnCloseMenu_Click(sender, e);

            Project prj = Project.GetInstance();
            prj.Name = txtPrjName.Text;

            DateTime now = DateTime.Now;
            string path = now.ToString("yyMMMdd_HHmmss");

            if (!Directory.Exists(C_SAVE_PATH))
                Directory.CreateDirectory(C_SAVE_PATH);

            prj.Save(C_SAVE_PATH + @"\" + path + Properties.Resources.extProject);

            PerperationWindow pw = new PerperationWindow();
            pw.Show();
            App.Current.MainWindow = pw;
            this.Close();
        }

        private void btnCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            __gridMenu.Visibility = Visibility.Hidden;
            __gridMenu = null;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            station.Cancel();
        }
    }
}
