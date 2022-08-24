using System;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using System.Collections;

namespace EDKv5.Utility.Scanners
{
    public class MSOfiiceScanEngine : IScanEngine
    {
        public dynamic[,] ReadExcel(string path, string[] colNames, out int rowCount, out int colCount)
        {
            dynamic[,] table;

            Application xls = null;
            Workbooks wbs = null;
            Workbook wbk = null;
            Sheets wss = null;
            Worksheet wsh = null;
            Range usedRange = null;

            try
            {
                //create all COM Objects
                xls = new Application();
                wbs = xls.Workbooks;
                wbk = wbs.Open(path);

                wss = wbk.Worksheets;
                IEnumerator wer = wss.GetEnumerator();
                wer.MoveNext();
                wsh = (Worksheet)wer.Current;
                usedRange = wsh.UsedRange;

                //remove other COM Sheets
                while (wer.MoveNext())
                    Marshal.FinalReleaseComObject(wer.Current);

                //get size
                //get row size
                Range rows = usedRange.Rows;
                rowCount = rows.Count;
                Marshal.FinalReleaseComObject(rows);
                rows = null;                           //release COM object

                //get column size
                Range cols = usedRange.Columns;
                colCount = cols.Count;
                Marshal.FinalReleaseComObject(cols);
                cols = null;                           //release COM object

                //convert Com Objects to data values
                table = new dynamic[rowCount, colCount];

                //copy data
                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        var cell = usedRange[r + 1, c + 1];
                        table[r, c] = cell.Value;
                        Marshal.FinalReleaseComObject(cell);
                        cell = null;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw ex;
            }
            finally
            {
                //close file
                wbk.Close(false);
                xls.Quit();

                //COM release
                Marshal.FinalReleaseComObject(usedRange);
                Marshal.FinalReleaseComObject(wss);
                Marshal.FinalReleaseComObject(wsh);
                Marshal.FinalReleaseComObject(wbk);
                Marshal.FinalReleaseComObject(wbs);
                Marshal.FinalReleaseComObject(xls);

                //GC release
                usedRange = null;
                wss = null;
                wsh = null;
                wbk = null;
                wbs = null;
                xls = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            //released all COM object reference

            return table;
        }
    }
}
