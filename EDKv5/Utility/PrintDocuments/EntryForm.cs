using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5.Utility.PrintDocuments
{
    public class EntryForm : PrintDocument
    {
        //constructor
        public EntryForm(Project project)
        {
            BeginPrint += _beginPrint;
            PrintPage += _printPage;
            clsStu = project.ClassStudents;
            prjYear = project.Year;
            events = project.NonRelayEvents;
        }

        public string StudentNameColumn = "Student Name";
        public string FormName = " Application Form";
        public string FirstPageName = "Column-Events Reference Table";

        static Font font = new Font("sans-serif, monospace", 12f, FontStyle.Regular);
        static Font fontTitle = new Font("sans-serif, monospace", 14f, FontStyle.Bold | FontStyle.Underline);

        int prjYear;
        Event[] events;
        Dictionary<Class, List<Student>> clsStu;
        IEnumerator<KeyValuePair<Class, List<Student>>> enumerator;

        bool firstPagePrinted;
        protected void _beginPrint(object sender, PrintEventArgs e)
        {
            enumerator = clsStu.GetEnumerator();
            enumerator.MoveNext();
        }
        protected void _printPage(object sender, PrintPageEventArgs e)
        {
            if (firstPagePrinted)
            {
                KeyValuePair<Class, List<Student>> t = enumerator.Current;

                //get code
                string clsYrCode = string.Format("C{0}{1}", t.Key.Key, prjYear);

                printSinglePage(e.Graphics, e.PageBounds, clsYrCode, t.Value.ToArray(), events);

                //next;
                e.HasMorePages = enumerator.MoveNext();
            } 
            else
            {
                Project prj = Project.GetInstance();
                Graphics g = e.Graphics;

                RectangleF rect = e.PageBounds;

                string firstLine = prj.Name + FormName;
                SizeF sz = g.MeasureString(firstLine, fontTitle);
                PointF pt = new PointF((rect.Width - sz.Width) / 2f, 50f);
                g.DrawString(firstLine, fontTitle, Brushes.Black, pt);

                sz = g.MeasureString(FirstPageName, fontTitle);
                pt = new PointF((rect.Width - sz.Width) / 2f, pt.Y + sz.Height + 10f);
                g.DrawString(FirstPageName, fontTitle, Brushes.Black, pt);

                pt.Y += sz.Height + 10f;
                pt.X = 30f;

                char c = 'A';
                foreach (Event ev in events)
                {
                    g.DrawString(c.ToString(), font, Brushes.Black, pt);
                    g.DrawString(ev.Name, font, Brushes.Black, pt.X + 20f, pt.Y); 
                    pt.Y += 28f;
                    c++;
                }

                firstPagePrinted = true;
                e.HasMorePages = true;
            }

        }
        private void printSinglePage(Graphics g, Rectangle pageBounds, string clsYrCode, Student[] students, Event[] events)
        {
            const int padLeft = 30;
            const int padTop = 95;

            const int cellPad = 4;
            const int cellHeight = 16;
            const int cellTH = cellPad + cellPad + cellHeight;

            const int codeHeight = 6;
            const int codeElmWidth = 2;

            int stuCnt = students.Length;
            int evCnt = events.Length;

            //make check sum
            byte[] cmpByts = Encoding.ASCII.GetBytes(" " + clsYrCode);
            int sum = 0;
            foreach (char c in clsYrCode)
                sum += c;
            cmpByts[0] = (byte)(sum % 256);

            //graphics objects
            Rectangle rectCode = new Rectangle(padLeft, padTop - codeHeight * 2 - cellPad, codeElmWidth, codeHeight);

            //draw string
            Project prj = Project.GetInstance();
            string firstLine = prj.Name + FormName;
            SizeF sz = g.MeasureString(firstLine, fontTitle);
            PointF pt = new PointF((pageBounds.Width - sz.Width) / 2f, 50f);
            g.DrawString(firstLine, fontTitle, Brushes.Black, pt);

            
            string clsYr = clsYrCode.Insert(3, "-");
            sz = g.MeasureString(clsYr, font);
            pt = new Point(pageBounds.Width - padLeft - (int)sz.Width, rectCode.Y);
            g.DrawString(clsYr, font, Brushes.Black, pt);

            //draw code
            g.DrawLine(Pens.Black, rectCode.Location, new PointF(rectCode.Left + codeElmWidth * 8 * cmpByts.Length, rectCode.Top));
            g.DrawLine(Pens.Red, rectCode.Left, rectCode.Bottom + codeHeight, rectCode.Left + codeElmWidth * 8 * cmpByts.Length, rectCode.Bottom + codeHeight);

            Random r = new Random();
            byte hash = (byte)r.Next(0, 256);
            foreach (byte b in cmpByts)
            {
                hash ^= b;
                byte l = b;
                byte h = hash;
                for (int j = 0; j < 8; j++)
                {
                    if (0 < l % 2)
                        g.FillRectangle(Brushes.Black, rectCode);
                    if (0 < h % 2)
                        g.FillRectangle(Brushes.Black, rectCode.Left, rectCode.Bottom, rectCode.Width, rectCode.Height);

                    l >>= 1;
                    h >>= 1;
                    rectCode.Location += new Size(codeElmWidth, 0);
                }
            }

            //prepare student table
            Rectangle rectTable = pageBounds;

            //set table frame
            rectTable.Location += new Size(padLeft, padTop);
            rectTable.Width = 240 + cellTH * evCnt - padLeft;
            rectTable.Height = (stuCnt + 1) * cellTH;

            //draw table frame
            g.DrawRectangle(Pens.Black, rectTable);

            //draw table columns
            g.DrawLine(Pens.Black, padLeft + 30f, padTop, padLeft + 30f, padTop + rectTable.Height);

            float y = padTop;

            //draw heading
            y += cellPad;   //padding
            g.DrawString("#", font, Brushes.Black, padLeft + 5f, y);
            g.DrawString(StudentNameColumn, font, Brushes.Black, padLeft + 32f, y);
            for (int i = 0; i < evCnt; i++)
            {
                float x = 240 + (i * cellTH);
                //draw event column lines
                g.DrawLine(Pens.Black, x, padTop, x, padTop + rectTable.Height);
                //draw event column name
                g.DrawString(((char)(i + 65)).ToString(), font, Brushes.Black, x + 2f, y);
            }
            y += cellHeight;  //height
            y += cellPad;     //padding
            g.DrawLine(Pens.Black, padLeft, y, rectTable.Width + padLeft, y);

            //loop over students
            foreach (Student stu in students)
            {
                y += cellPad;       //padding

                //class number - pad right
                float clsNoX = padLeft + 28f;                               //init right
                string clsNoStr = stu.ClassNo.ToString();                   //get string value
                SizeF clsNoSz = g.MeasureString(clsNoStr, font);            //measure string width
                clsNoX -= clsNoSz.Width;                                    //move top left
                g.DrawString(clsNoStr, font, Brushes.Black, clsNoX, y);

                //student name
                g.DrawString(stu.Name, font, Brushes.Black, padLeft + 32f, y);

                y += cellHeight;                                            //height
                y += cellPad;                                               //padding
                g.DrawLine(Pens.Black, padLeft, y, rectTable.Width + padLeft, y);
            }
        } // end void printSinglePage
    } // end class
}
