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
using System.Windows.Shapes;

using EDKv5;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for PerperationWindow.xaml
    /// </summary>
    public partial class PerperationWindow : Window
    {
        public PerperationWindow()
        {
            InitializeComponent();

            Project prj = Project.GetInstance();
            btnStuInfo.Tag = stuPg = new StudentInfoPage();
            btnHsInfo.Tag = hsPg = new HouseSettingPage(new HousePanel());
            btnEvInfo.Tag = evPg = new EventSettingPage();
            btnScheduler.Tag = schPg = new SchedulerPage();
            btnHolder.Tag = hdPg = new HolderSettingPage(new ResultHolderPanel());

            curPage = (CurrentPage)FindResource("curPage");

        }

        CurrentPage curPage;

        //--
        StudentInfoPage stuPg;
        HouseSettingPage hsPg;
        EventSettingPage evPg;
        SchedulerPage schPg;
        HolderSettingPage hdPg;
        /**/

        //=============================================================
        // title bar operations
        #region Title Bar Operations
        private void _dragMoveThis(object sender, MouseEventArgs e) { this.DragMove(); }
        private void btnClose_Click(object sender, RoutedEventArgs e) { this.Close(); }
        private void btnMinimize_Click(object sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }
        #endregion

        //=============================================================
        private void btnSwitchPage_Click(object sender, RoutedEventArgs e)
        {
            var obj = main.Child;
            IPage page = obj as IPage;
            page?.BeforeOut();
            page = (IPage)((Button)sender).Tag;
            page.In();
            curPage.Page = (wPage)page;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Project prj = Project.GetInstance();
            prj.Save();
        }
        
    }

}
