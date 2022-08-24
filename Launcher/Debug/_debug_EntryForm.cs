
#if TEST
using System.Drawing;
using System.Drawing.Printing;

using EDKv5;

namespace Launcher.Debug
{

    class EntryForm : PrintDocument
    {
        private class _adpt_EntryForm : EDKv5.Utility.PrintDocuments.EntryForm
        {
            public _adpt_EntryForm(Project project) : base(project) { }

            public void beginPrint(object sender, PrintEventArgs e) { _beginPrint(sender, e); }
            public void printPage(object sender, PrintPageEventArgs e) { _printPage(sender, e); }
        }

        public EntryForm(Project project) : base()
        {
            ef = new _adpt_EntryForm(project);
            BeginPrint += _beginPrint_decorate;
            PrintPage += _printPage_decorate;
        }

        // field
        _adpt_EntryForm ef;
        int k;

        private void _beginPrint_decorate(object sender, PrintEventArgs e)
        {
            ef.beginPrint(sender, e);
            k = 0;
        }
        private void _printPage_decorate(object sender, PrintPageEventArgs e)
        {
            //create image
            Image img = new Bitmap(e.PageBounds.Width, e.PageBounds.Height);
            Graphics g = Graphics.FromImage(img);
            g.Clear(Color.White);

            PrintPageEventArgs a = new PrintPageEventArgs(g, e.MarginBounds, e.PageBounds, e.PageSettings);

            //adapt to draw on image
            ef.printPage(sender, a);

            //redraw image to actual print document
            e.Graphics.DrawImage(img, 0, 0, e.PageBounds.Width, e.PageBounds.Height);

            //copy next page setting
            e.HasMorePages = a.HasMorePages;

            //save image
            img.Save(@"R:\fr" + (k++).ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
        }
    }

}
#endif