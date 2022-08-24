using System.Windows;
using System.Windows.Documents;

namespace Launcher
{
    // ----
    // http://stackoverflow.com/questions/4857754/wpf-flowdocument-stretch-table-to-entire-width

    public class PrintLayout
    {
        public static readonly PrintLayout A3 = new PrintLayout("29.7cm", "42cm", "3.18cm", "2.54cm");
        public static readonly PrintLayout A3Narrow = new PrintLayout("29.7cm", "42cm", "1.27cm", "1.27cm");
        public static readonly PrintLayout A3Moderate = new PrintLayout("29.7cm", "42cm", "1.91cm", "2.54cm");

        public static readonly PrintLayout A4 = new PrintLayout("21cm", "29.7cm", "2.54cm", "1.91cm");

        public static readonly PrintLayout A5 = new PrintLayout("21cm", "14.8cm", "2cm", "1.45cm");


        public PrintLayout(string w, string h, string leftright, string topbottom)
            : this(w, h, leftright, topbottom, leftright, topbottom)
        {
        }

        public PrintLayout(string w, string h, string left, string top, string right, string bottom)
        {
            var converter = new LengthConverter();
            var width = (double)converter.ConvertFromInvariantString(w);
            var height = (double)converter.ConvertFromInvariantString(h);
            var marginLeft = (double)converter.ConvertFromInvariantString(left);
            var marginTop = (double)converter.ConvertFromInvariantString(top);
            var marginRight = (double)converter.ConvertFromInvariantString(right);
            var marginBottom = (double)converter.ConvertFromInvariantString(bottom);
            this.Size = new Size(width, height);
            this.Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom);
        }

        public Thickness Margin { get; set; }

        public Size Size { get; }

        public double ColumnWidth
        {
            get
            {
                var column = 0.0;
                column = this.Size.Width - Margin.Left - Margin.Right;
                return column;
            }
        }
    } // end class PrintLayout

    public static class PrintExtension
    {
        public static void SetLayout(this FlowDocument document, PrintLayout layout)
        {
            document.PageWidth = layout.Size.Width;
            document.PageHeight = layout.Size.Height;
            document.PagePadding = layout.Margin;
            document.ColumnWidth = layout.ColumnWidth;
        }
    }
}
