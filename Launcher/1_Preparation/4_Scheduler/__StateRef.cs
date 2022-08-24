using EDKv5.MonitorServices;
using System.Windows;

namespace Launcher
{
    class __StateRef : DependencyObject
    {
        public static DependencyProperty ValueProperty = DependencyProperty.Register("State",
            typeof(EventCompletionState), typeof(__StateRef), new PropertyMetadata(EventCompletionState.None, _on_ValueChanged));

        public event DependencyPropertyChangedEventHandler ValueChanged;

        private static void _on_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((__StateRef)d).OnValueChanged(e);
        }

        public EventCompletionState Value
        {
            get { return (EventCompletionState)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        protected void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            if (null != this.ValueChanged)
                this.ValueChanged(this, e);
        }

    }
}
