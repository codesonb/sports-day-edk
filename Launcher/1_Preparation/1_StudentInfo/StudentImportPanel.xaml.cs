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

using IO = System.IO;

using EDKv5;
using EDKv5.Utility;
using System.Threading.Tasks;
using Launcher.Scanners;
using Microsoft.Win32;

namespace Launcher
{
    public partial class StudentImportPanel : UserControl
    {
        public event EventHandler ScannerCompleted;
        public event EventHandler ScannerObscured;
        public event EventHandler ScannerCancelled;

        public StudentImportPanel()
        {
            InitializeComponent();

            gridMain.AllowDrop = true;

            ofd.Filter = Properties.Resources.filterStudentInfoExcelFile;
        }

        OpenFileDialog ofd = new OpenFileDialog();

        private void _DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string path in paths)
                {
                    string ext = IO.Path.GetExtension(path).ToLower();
                    switch (ext)
                    {
                        case ".xlsx":
                        case ".xls":
                        case ".csv":
                            e.Effects = DragDropEffects.Move;
                            return;
                    }
                }
            }
            e.Effects = DragDropEffects.None;
        }

        private void _Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string path in paths)
                {
                    string ext = IO.Path.GetExtension(path).ToLower();
                    switch (ext)
                    {
                        case ".xlsx":
                        case ".xls":
                        case ".csv":
                            loadFile(path);
                            return;
                    }
                }
            }
        }
        private void _MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (true == ofd.ShowDialog())
                loadFile(ofd.FileName);
        }

        private void loadFile(string path)
        {
            // setup UI display
            imgDropHint.Visibility = Visibility.Collapsed;
            vboxLoading.Visibility = Visibility.Visible;

            // disable drop
            gridMain.AllowDrop = false;

            Project prj = Project.GetInstance();

            AskInfoPanel pnl = new AskInfoPanel();
            IAskFirstRow afr = pnl;
            IManualMatch matcher = pnl;
            pnl.RequestShow += this.onScannerObscured;


            // handler setup
            StudentInfoScanner stuScanner = new StudentInfoScanner(afr, matcher);   //update handler


            // read excel file on new thread
            Task th = new Task(() =>
            {
                try
                { 
                    // standard load
                    prj.LoadStudents(path, stuScanner);

                    // cancel
                    if (pnl.Cancelled && null != this.ScannerCancelled)
                    {
                        this.ScannerCancelled(stuScanner, EventArgs.Empty);
                        return;
                    }

                    // observer event callback
                    if (null != ScannerCompleted)
                        this.ScannerCompleted(stuScanner, EventArgs.Empty);
                }
                catch (IO.IOException ex)
                {
                    // resume the UI
                    MessageBox.Show("Cannot read the file.\nPlease make sure that you are not openning the Excel file, and the file is not corrupted.", "EDKv5 - Something wrong", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                finally
                {
                    Dispatcher.Invoke(new Action(() => {
                        vboxLoading.Visibility = Visibility.Collapsed;
                        imgDropHint.Visibility = Visibility.Visible;
                    }));
                }
            });
            th.Start(); // start thread
        } // end void


        private void onScannerObscured(object sender, EventArgs e) { this.ScannerObscured(sender, e); }

    } // end class
} // end namespace
