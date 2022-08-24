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
using EDKv5.Utility.Scanners;
using Launcher.Scanners;

using IO = System.IO;
using D = System.Drawing;
using EDKv5.Utility.PrintDocuments;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for StudentInfoPanel.xaml
    /// </summary>

    public partial class LaneAssignPanel : UserControl, IPageSwitch, IPage
    {
        public event EventHandler CallPreviousPage;
        public event EventHandler CallNextPage;

        //constructor
        #region Initialization
        public LaneAssignPanel()
        {
            InitializeComponent();
        }
        #endregion

        // fields
        #region Fields
        
        #endregion

        #region Main Operation
        
        #endregion

        #region Main UI Mouse Drag
        private void laneItm_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseButtonState.Pressed == e.LeftButton)
            {
                FrameworkElement s = (FrameworkElement)sender;
                DragDrop.DoDragDrop(s, s, DragDropEffects.Move);
            }
        }
        private void laneItm_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListBoxItem)))
            {
                // get original
                var ori = (ListBoxItem)e.Data.GetData(typeof(ListBoxItem));
                var source = ori.ClosestParent<ListBox>().ItemsSource;

                // get target destination
                var target = (ListBoxItem)sender;
                var parent = target.ClosestParent<ListBox>();

                // execute only if source = destination, or target is not full
                if (parent == source || 8 > parent.Items.Count)
                {
                    try
                    {
                        var stu = (Participant)ori.DataContext;
                        var oriList = (ObservableCollectionAdapter<Participant>)source;
                        int index = ((IList<Participant>)parent.ItemsSource).IndexOf((Participant)target.DataContext);

                        // remove from original
                        oriList.Remove(stu);

                        // then add
                        var newList = (ObservableCollectionAdapter<Participant>)parent.ItemsSource;
                        newList.Insert(index, stu);
                    } catch (Exception ex) { /* do nothing */ }
                }
            }
        }
        #endregion

        #region UI Button

        #endregion

        #region UI Updates
        private void _updateEventList()
        {
            Project prj = Project.GetInstance();
            lsEv.ItemsSource = prj.Events.Where((e) => { return e.NeedLaneAssignment; });
        }
        #endregion
        //-----

        //private void btnNext_Click(object sender, RoutedEventArgs e)
        //{
        //    this.CallNextPage?.Invoke(this, e);
        //}
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            this.CallPreviousPage?.Invoke(this, e);
        }


        #region Interface Implementation
        UIElement IPage.UI { get { return this; } }
        void IPage.In() { this._updateEventList(); }
        void IPage.BeforeOut() { }
        #endregion

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = Properties.Resources.filterExcelFile;

            if (true == sfd.ShowDialog())
            {
                using (IO.Stream stream = sfd.OpenFile())
                {
                    var project = Project.GetInstance();
                    ParticipationExcelWriter.Write(project, stream);
                }
            }
        }
    }
}
