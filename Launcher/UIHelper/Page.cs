using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Launcher
{
    interface IPage
    {
        void In();
        void BeforeOut();
        UIElement UI { get; }
    }

    abstract class wPage : DependencyObject, IPage
    {
        public static readonly DependencyProperty UIProperty =
            DependencyProperty.Register("UI", typeof(UIElement), typeof(wPage), new PropertyMetadata(null));

        public wPage(UserControl UI) { this.UI = UI; }
        public virtual void In() { }
        public virtual void BeforeOut() { }
        public virtual UIElement UI {
            get { return (UIElement)GetValue(UIProperty); }
            protected internal set { SetValue(UIProperty, value); }
        }
    }

    class CurrentPage : DependencyObject, IPage
    {
        public static readonly DependencyProperty UIProperty =
            DependencyProperty.Register("UI", typeof(UIElement), typeof(CurrentPage), new PropertyMetadata(null));

        // properties
        public virtual void In() { }
        public virtual void BeforeOut() { }

        public UIElement UI
        {
            get { return (UIElement)GetValue(UIProperty); }
            protected internal set { SetValue(UIProperty, value); }
        }

        internal wPage Page
        {
            set
            {
                //this.UI = value.UI;
                Binding bind = new Binding();
                bind.Source = value;
                bind.Path = new PropertyPath("UI");
                BindingOperations.SetBinding(this, UIProperty, bind);
            }
        }
    }
}
