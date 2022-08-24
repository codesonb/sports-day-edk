using System.IO;
using OfficeOpenXml;

namespace EDKv5.Utility.Scanners
{
    class XmlScanEngine : IScanEngine
    {

        public dynamic[,] ReadExcel(string path, string[] colNames, out int rowCount, out int colCount)
        {
            dynamic[,] table;

            using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read))
            using (ExcelPackage pck = new ExcelPackage(stream))
            using (ExcelWorkbook wbk = pck.Workbook)
            using (ExcelWorksheet wsh = wbk.Worksheets[1])
            {
                colCount = 0;
                rowCount = 0;

                int space = 0;
                int emp = 0;
                while (true)
                {
                    if (null != wsh.Cells[1, colCount + 1].Value)
                    {
                        colCount++;
                        space = 0;
                        if (emp < 1) { emp = colCount; }
                    }
                    else if (++space < 5) { colCount++; }
                    else { break; }
                }

                while (null != wsh.Cells[rowCount + 1, emp].Value)
                    rowCount++;

                table = new dynamic[rowCount, colCount];

                //copy data
                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        table[r, c] = wsh.Cells[r + 1, c + 1].Value;
                    }
                }

            }

            return table;
        }
    }
}
