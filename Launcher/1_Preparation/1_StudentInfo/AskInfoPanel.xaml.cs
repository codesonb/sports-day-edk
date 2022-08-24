using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
using System.Threading;

namespace Launcher
{
    public partial class AskInfoPanel : UserControl, IAskFirstRow, IManualMatch
    {
        public event EventHandler ScannerCompleted;
        public event EventHandler RequestShow;

        // constructor
        public AskInfoPanel()
        {
            InitializeComponent();
        }

        // fields
        Semaphore semaphore;

        // properties
        public bool FirstRowIsHeading { get; private set; }
        public bool Cancelled { get; private set; }
        public short[] Result { get; private set; }

        bool _select_col;
        int _select_idx;
        IEnumerator enumerator;

        // function
        private void _init_ask_first_row(dynamic[,] table, string[] colNames)
        {
            //-- set state
            _select_col = false;

            //-- display button
            btnYes.Visibility = btnNo.Visibility = Visibility.Visible;

            //-- copy table to 1-to-1 dimension
            DataTable dyTable = new DataTable("Excel Data");
            for (int i = 0; i <= table.GetUpperBound(1); i++)
                dyTable.Columns.Add(new DataColumn("Column " + (i + 1)));

            int top10 = Math.Min(table.GetUpperBound(0), 10);
            for (int r = 0; r <= top10; r++)
            {
                DataRow row = dyTable.NewRow();
                row.ItemArray = table.CopyRow(r);
                dyTable.Rows.Add(row);
            }

            //-- pass auto visualization
            lvDataPreview.DataContext = dyTable.DefaultView;

            //-- display update
            this.RequestShow(this, EventArgs.Empty);
        }

        //-------------------------------
        private void _init_match(Tuple<string, int>[] keys, string[] values)
        {
            //-- set state
            _select_col = true;

            // display buttons
            btnYes.Visibility = btnNo.Visibility = Visibility.Collapsed;

            // update selection
            _select_idx = -1;
            Result = new short[keys.Length];
            enumerator = keys.GetEnumerator();
            _match_proceed();
        }
        private void _match_proceed()
        {
            while (enumerator.MoveNext())
            {
                var x = (Tuple<string, int>)enumerator.Current;
                Result[++_select_idx] = (short)x.Item2;
                if (x.Item2 < 0)
                {
                    lbDescription.Text = "Please click on the column of " + x.Item1;
                    return;
                }
            }
            semaphore.Release();
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            FirstRowIsHeading = true;
            semaphore.Release();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            FirstRowIsHeading = false;
            semaphore.Release();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Cancelled = true;
            if (null != semaphore) { semaphore.Release(); }
        }

        //-------------------------------
        #region Explicit Interface Implementation
        void IAskFirstRow.Initialize(dynamic[,] table, string[] colNames, Semaphore semaphore)
        {
            this.semaphore = semaphore;
            Dispatcher.Invoke(new Action(() => { _init_ask_first_row(table, colNames); }));
        }

        void IManualMatch.Initialize(Tuple<string, int>[] keys, string[] values, Semaphore semaphore)
        {
            this.semaphore = semaphore;
            Dispatcher.Invoke(new Action(() => { _init_match(keys, values); }));
        }
        #endregion


        private void lvDataPreview_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_select_col) return;

            //--
            FrameworkElement f = (FrameworkElement)e.OriginalSource;
            var target = f.ClosestParent<DataGridCell>();
            if (null != target)
            {
                Result[_select_idx] = (short)target.Column.DisplayIndex;
                _match_proceed();
            }

        } // end void lvDataPreview_MouseUp
    }
}
