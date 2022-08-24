using EDKv5;
using EDKv5.MonitorServices;
using System.Windows;

namespace Launcher
{
    internal class CompetingGroupVisualAdapter : DependencyObject
    {
        public static DependencyProperty StateProperty = DependencyProperty.Register("State",
            typeof(EventCompletionState), typeof(CompetingGroupVisualAdapter));

        // constructor
        public CompetingGroupVisualAdapter(int row, int col, int span, CompetingGroup competingGroup, __StateRef stateObj)
        {
            //-----
            this.Row = row;
            this.CompetingGroup = competingGroup;
            this.Column = col;
            this.Span = span;
            this._state = stateObj;
            //-----
            this._state.ValueChanged += (s, e) => { CallValueUpdated(e); };
        }

        /// fields
        __StateRef _state;

        /// property
        public CompetingGroup CompetingGroup { get; }
        public int Row { get; }
        public int Column { get; }
        public int Span { get; }
        public EventCompletionState State
        {
            get { return _state.Value; }
            set
            {
                _state.Value = value;
                SetValue(StateProperty, value);
            }
        }

        private void CallValueUpdated(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                SetValue(StateProperty, e.NewValue);
            }
        }

    }
}
