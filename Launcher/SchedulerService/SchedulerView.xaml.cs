using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using EDKv5;
using EDKv5.SchedulerService;
using EDKv5.MonitorServices;
using System.Windows.Media;

namespace Launcher
{
    public partial class SchedulerView : UserControl
    {
        internal event Func<CompetingGroupVisualAdapter, EventCompletionState> ItemClicked;

        #region Initialization
        public SchedulerView()
        {
            InitializeComponent();

            //------------
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.DragEnter += _onDragEnter;
                this.Drop += _onDrop;
                this.Loaded += (s, e) =>
                {
                    //---
                    var b = displayContainer.ApplyTemplate();
                    var c = displayContainer.First<ItemsPresenter>();
                    var d = c.ApplyTemplate();
                    grid = (Grid)displayContainer.ItemsPanel.FindName("mainGrid", c);
                    //---
                    moStyle = (Style)grid.Resources["moStyle"];
                    //---
                    Update();
                };
            }
        }

        public void Init(IScheduleDay day, bool cMode)
        {
            this.Day = day;
            this.cMode = cMode;
            Update();
        }
        #endregion

        // fields
        #region Fields
        Grid grid;
        Style moStyle;
        bool cMode;
        bool eMode;
        SortedList<int, __StateRef> compStates = new SortedList<int, __StateRef>();
        #endregion

        // properties
        #region Properties
        public IScheduleDay Day { get; private set; }
        public IPeriod GetPeriod(Point point)
        {
            int row = -1;
            double start = 0;
            foreach (RowDefinition rd in grid.RowDefinitions)
            {
                start += rd.ActualHeight;
                if (point.Y < start) { return Day.Periods[row]; }
                row++;
            }
            return null;
        }
        public bool EditMode
        {
            get { return eMode; }
            set
            {
                  eMode
                = AllowDrop
                = value;
            }
        }
        #endregion

        // functions
        #region Main UI Update
        public void Update()
        {
            // skip if UI is not loaded
            if (!this.IsLoaded) { return; }

            List<object> ls = new List<object>();
            _reset_grid_frame(ls);  // init frame
            _init_grid_content(ls); // init content

            displayContainer.ItemsSource = ls;

        } //end void Init()
        private void _reset_grid_frame(List<object> ls)
        {
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

            int colCnt = cMode ? 7 : 9;
            for (int i = 0; i < colCnt; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition());

            //init header row
            var rowDef = new RowDefinition() { MaxHeight = 30 };
            grid.RowDefinitions.Add(rowDef);

            ls.Add(new SimpleStringCell(0, 0, 1, ""));
            if (cMode)
            {
                ls.AddRange(new object[] {
                    new SimpleStringCell(0, 1, 1, "FC"),
                    new SimpleStringCell(0, 2, 1, "FB"),
                    new SimpleStringCell(0, 3, 1, "FA"),
                    new SimpleStringCell(0, 4, 1, "MC"),
                    new SimpleStringCell(0, 5, 1, "MB"),
                    new SimpleStringCell(0, 6, 1, "MA")
                });
            }
            else
            {
                ls.AddRange(new object[] {
                    new SimpleStringCell(0, 1, 1, "FD"),
                    new SimpleStringCell(0, 2, 1, "FC"),
                    new SimpleStringCell(0, 3, 1, "FB"),
                    new SimpleStringCell(0, 4, 1, "FA"),
                    new SimpleStringCell(0, 5, 1, "MD"),
                    new SimpleStringCell(0, 6, 1, "MC"),
                    new SimpleStringCell(0, 7, 1, "MB"),
                    new SimpleStringCell(0, 8, 1, "MA")
                });
            }
        }
        private void _init_grid_content(List<object> ls)
        {
            int _t_fullBarSize = cMode ? 6 : 8;

            int row = 0;
            DateTime cur = Day.Start;
            foreach (IPeriod period in Day.Periods)
            {
                var rowDef = new RowDefinition() { Height = new GridLength(40) };
                grid.RowDefinitions.Add(rowDef); // create row
                ++row;  // update index

                DateTime nxt = cur.AddMinutes(period.Length);
                ls.Add(new SimpleStringCell(row, 0, 1, cur.ToString("HH:mm") + "\n" + nxt.ToString("HH:mm")));
                cur = nxt;

                //init cells
                Group rowGroup = Group.None;
                foreach (Cell cell in period.Cells)
                {
                    Group group = cell.Group;
                    var data = cell.Data;
                    if (data is string)
                    {
                        var itm = new SimpleStringCell(row, 1, _t_fullBarSize, (string)data);
                        ls.Add(itm);
                    }
                    else if (data is CompetingGroup)
                    {
                        var cmpGrp = (CompetingGroup)data;
                        var itms = _init_Cell(row, group, cmpGrp, cMode);
                        ls.AddRange(itms);
                    }
                    else
                        throw new Exception("Unknown type");
                    rowGroup |= group;
                }

                uint empty = (uint)~rowGroup & 0xFF;
                if (cMode) { empty = _cMode_bitShift(empty); }
                int index = 1;
                while (empty > 0)
                {
                    if (0 < (empty & 1))
                    {
                        var box = new SimpleStringCell(row, index, 1, "");
                        ls.Add(box);
                    }
                    index++;
                    empty >>= 1;
                }

            } // end for row-period
        }
        private CompetingGroupVisualAdapter[] _init_Cell(int row, Group group, CompetingGroup competingGroup, bool cMode)
        {
            List<CompetingGroupVisualAdapter> ls = new List<CompetingGroupVisualAdapter>();
            uint b = (uint)group;
            if (b <= 0) return null;

            __StateRef stateRef = new __StateRef();
            _register_compState(competingGroup, stateRef);
            int readBits = 9;

            //reversed load
            uint gpbit = (uint)b & 0xFFu;
            if (cMode) //convert cMode specialized
            {
                readBits -= 2;
                gpbit = _cMode_bitShift(gpbit);
            }
            gpbit <<= 1;                //first shift => 0~7 index = 1~8

            { // BLOCK
              /* Multipurposed - optimized variable // longer-life stepper */
                int end;                //end position span index = last position + 1 = current stepper
                int start = 0;

                bool flag = false;  //convex signal
                for (end = 0; end < readBits; end++) //neglect first bit
                {
                    uint x = 1 & gpbit;

                    if (flag) //high
                    {
                        if (0 == x)
                        {
                            //Label label = _init_Cell_create_UI(row, start, end - start, competingGroup);
                            //label.Style = style;
                            ls.Add(new CompetingGroupVisualAdapter(row, start, end - start, competingGroup, stateRef));
                            flag = false;
                        }
                    }
                    else
                    {
                        flag = 0 < x;
                        if (flag)
                            start = end;    // i = index[1..8]
                    }

                    gpbit >>= 1; //shift next
                }

                if (flag) //last close required
                {
                    ls.Add(new CompetingGroupVisualAdapter(row, start, end - start, competingGroup, stateRef));
                }


                return ls.ToArray();
            } // # end block
        } // end void _init_Cell (CompetingGroup)
        private uint _cMode_bitShift(uint inBits)
        {
            uint l = inBits & 0x0Eu;   // 0000 1110  [  low 4-bits ]
            l >>= 1;                   // 0000 0111
            uint h = inBits & 0xE0u;   // 1110 0000  [ high 4-bits ]
            h >>= 2;                   // 0011 1000
            return h | l;              // 0011 1111  [   combine   ]
        }
        #endregion

        #region Drag Event Handle
        private void _onDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(CompetingGroup[])))
                e.Effects = DragDropEffects.Move;
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }
        private void _onDrop(object sender, DragEventArgs e)
        {
            CompetingGroup[] grps = (CompetingGroup[])e.Data.GetData(typeof(CompetingGroup[]));

            Project prj = Project.GetInstance();

            // get period
            Point pt = e.GetPosition(grid);
            ICompetingPeriod period = GetPeriod(pt) as ICompetingPeriod;

            if (null == period) return; // ignore assemble period

            int sucCount = prj.Schedule.Assign(period, grps);
            if (sucCount > 0) { this.Update(); }
        }
        private void _occupied_dragStart(object sender, MouseEventArgs e)
        {
            if (MouseButtonState.Pressed == e.LeftButton)
            {
                var s = (FrameworkElement)e.Source;
                if (s.DataContext is CompetingGroupVisualAdapter)       // prevent click event (removal) trigger this event
                {
                    var data = (CompetingGroupVisualAdapter)s.DataContext;
                    DragDrop.DoDragDrop(
                        (DependencyObject)sender,
                        new CompetingGroup[] { data.CompetingGroup },
                        DragDropEffects.Move
                    );
                }
            }
        }

        #endregion

        #region UI Display State
        private void _occupied_cellHoverStart(object sender, MouseEventArgs e)
        {
            if (eMode)
            {
                var s = (FrameworkElement)sender;
                var d = (CompetingGroupVisualAdapter)s.DataContext;
                d.State = EventCompletionState.Hover;
            }
        }

        private void _occupied_cellHoverEndRecover(object sender, MouseEventArgs e)
        {
            if (eMode)
            {
                var s = (FrameworkElement)sender;
                if (s.DataContext is CompetingGroupVisualAdapter)   // prevent click event trigger this part
                {
                    var d = (CompetingGroupVisualAdapter)s.DataContext;
                    d.State = EventCompletionState.None;
                }
            }
        }
        #endregion

        #region Removal
        private void _editor_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = !EditMode;
        }
        private void _removeItem_doubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.EditMode) { this._removeItem_menuClick(sender, e); }
        }
        private void RevertScheduledItem(CompetingGroup cpGrp)
        {
            Project prj = Project.GetInstance();
            Schedule sch = prj.Schedule;
            sch.Revert(cpGrp);
            Update();
        }
        private void _removeItem_menuClick(object sender, RoutedEventArgs e)
        {
            // revert
            var s = (FrameworkElement)sender;
            var d = (CompetingGroupVisualAdapter)s.DataContext;
            RevertScheduledItem(d.CompetingGroup);
        }
        #endregion

        #region Competition State Update
        private void _register_compState(CompetingGroup competingGroup, __StateRef state)
        {
            if (compStates.ContainsKey(competingGroup.ID))
                compStates[competingGroup.ID] = state;
            else
                compStates.Add(competingGroup.ID, state);
        }
        internal void SetState(CompetingGroup competingGroup, EventCompletionState stateValue)
        {
            SetState(competingGroup.ID, stateValue);
        }
        internal void SetState(int competingGroupID, EventCompletionState stateValue)
        {
            __StateRef stateObj;
            if (compStates.TryGetValue(competingGroupID, out stateObj))
                stateObj.Value = stateValue;
            else
                throw new InvalidOperationException("The competition group is not registered");
        }
        #endregion


        #region Mouse Handler
        private void _item_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var s = (FrameworkElement)sender;
            if (!(s.DataContext is CompetingGroupVisualAdapter)) return;

            if (MouseButton.Left == e.ChangedButton && MouseButtonState.Released == e.LeftButton)
            {
                var item = (CompetingGroupVisualAdapter)s.DataContext;
                if (null != this.ItemClicked)
                    item.State = this.ItemClicked(item);
            }
            else if (MouseButton.Right == e.ChangedButton && MouseButtonState.Released == e.RightButton)
            {

            }
        }

        #endregion

    }
}
