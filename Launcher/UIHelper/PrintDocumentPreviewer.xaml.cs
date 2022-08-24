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


namespace Launcher
{
    /// <summary>
    /// Interaction logic for PrintDocumentPreviewer.xaml
    /// </summary>
    public partial class PrintDocumentPreviewer : Window
    {
        public PrintDocumentPreviewer()
        {
            InitializeComponent();
        }

        // fields
        Thickness pagePaddingTmp;

        // properties
        public FlowDocument Document
        {
            set
            {
                const double C_MARGIN = 10;
                var D_MARGIN = new Thickness(0, C_MARGIN, 0, C_MARGIN);
                var cloneDoc = value.Clone();
                var blocks = cloneDoc.Blocks.ToArray();
                sizer.Width = value.PageWidth;
                sizer.Height = (value.PageHeight + C_MARGIN + C_MARGIN) * blocks.Length;

                var ls = new List<RichTextBox>();
                foreach (var blk in blocks)
                {
                    var rch = new RichTextBox()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = value.PageWidth,
                        Height = value.PageHeight,
                        Margin = D_MARGIN,
                        IsReadOnly = true,
                    };
                    rch.Loaded += load_pagePadding;
                    rch.Document.Blocks.Add(blk);
                    ls.Add(rch);
                }
                docContainer.ItemsSource = ls;

                pagePaddingTmp = value.PagePadding;
            }
        }

        private void load_pagePadding(object sender, RoutedEventArgs e)
        {
            ((RichTextBox)sender).Document.PagePadding = pagePaddingTmp;
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void docContainer_Scale(int delta)
        {
            if (delta > 0) { sizer.Width *= 1.11; }
            else if (delta < 0) { sizer.Width *= 0.9; }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.GetKeyStates(Key.LeftCtrl).HasFlag(KeyStates.Down) ||
                 Keyboard.GetKeyStates(Key.RightCtrl).HasFlag(KeyStates.Down))
            {
                docContainer_Scale(e.Delta);
                e.Handled = true;
            }
            else if (Keyboard.GetKeyStates(Key.LeftShift).HasFlag(KeyStates.Down) ||
                 Keyboard.GetKeyStates(Key.RightShift).HasFlag(KeyStates.Down))
            {
                if (e.Delta > 0) { ((ScrollViewer)sender).LineLeft(); }
                else { ((ScrollViewer)sender).LineRight(); }
                e.Handled = true;
            }
        }
    }
}
