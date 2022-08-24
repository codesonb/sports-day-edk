using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;

namespace EDKv5.Utility.Scanners
{
    public abstract class ExcelScanner<T>
    {
        protected abstract string[] getColumnNames();
        protected abstract string[] getColumnDisplayNames();

        protected abstract T objectCreationCallback(dynamic[] range, short[] index);
        protected abstract bool askFirstColumn(dynamic[,] rows);
        protected abstract bool askLackColumn(string[] requiredCols, dynamic[] valueRow, ref short[] index);
        protected virtual bool analyzeColumn(dynamic[][] row, ref short[] index) { index = new short[0]; return false; }
        protected abstract short[] optionalColumns { get; }

        public List<T> ReadDataset(string path)
        {
            string[] colNames = getColumnNames();

            bool mhh;           // may has header
            int rowCount;
            int colCount;
            int beginIdx = 0;
            dynamic[][] thirdRow;
            dynamic[,] table = _read_excel(path, colNames, out rowCount, out colCount);

            //set revised index
            short[] idx = new short[colNames.Length];
            for (int i = 0; i < idx.Length; i++) { idx[i] = -1; }   //init indexes

            if ((mhh = _check_header_row(table, colCount, colNames, ref idx)) &&
                _is_header_all_match(table, colCount, ref idx) ||
                _try_analyze_column(table, ref idx, out thirdRow) ||
                _ask_user(ref mhh, colNames, table, thirdRow, ref idx)
            ) {
                if (mhh) { beginIdx += 1; }
                return _do_convert(table, beginIdx, rowCount, idx);
            }
            else
            {
                // user cancel (?)
                return new List<T>();
            }

        } // end of public start read - initiator

        // open read
        private dynamic[,] _read_excel(string path, string[] colNames, out int rowCount, out int colCount)
        {
            IScanEngine scanEngine;
            string ext = System.IO.Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".csv":
                case ".xls":
                    scanEngine = new MSOfiiceScanEngine();
                    break;
                case ".xlsx":
                    scanEngine = new XmlScanEngine();
                    break;
                default:
                    throw new Exception("Not Supported Format");
            }

            return scanEngine.ReadExcel(path, colNames, out rowCount, out colCount);
        } // #end read excel

        private List<T> _do_convert(dynamic[,] table, int beginIdx, int rowCount, short[] idx)
        {
            List<T> ls = new List<T>();
            for (int i = beginIdx; i<rowCount; i++)
            {
                dynamic[] row = table.CopyRow(i);
                T obj = objectCreationCallback(row, idx);
                ls.Add(obj);
            }

            return ls;
        }

        private bool _check_header_row(dynamic[,] table, int colCount, string[] colNames, ref short[] idx)
        {
            //check first row column name get index
            for (int i = 0; i < colCount; i++)
            {
                //get first row cell value
                dynamic tmpValue = table[0, i];

                //non string cell means not no first row header
                if (null != tmpValue && !(tmpValue is string))
                {
                    while (i-- >= 0) { idx[i] = -1; }   // reverse recover changes
                    return false; // which indicates that the first row must NOT be headers
                }

                string colName = (string)tmpValue;
                for (short k = 0; k < colNames.Length; k++)
                    if (colNames[k] == colName) { idx[i] = k; break; }
            }

            return true;
        }

        private bool _is_header_all_match(dynamic[,] table, int colCount, ref short[] idx)
        {
            //check if columns all matched
            for (int i = 0; i < colCount; i++)
                if (idx[i] < 0) { return false; }  // check false false
            return true;
        }

        private bool _try_analyze_column(dynamic[,] table, ref short[] idx, out dynamic[][] thirdRow)
        {
            thirdRow = new dynamic[3][];
            thirdRow[0] = table.CopyRow(2);              // third row ~ do not use first 2 rows
            thirdRow[1] = table.CopyRow(3);              // forth row ~ do not use first 2 rows
            thirdRow[2] = table.CopyRow(4);              // fifth row ~ do not use first 2 rows

            return analyzeColumn(thirdRow, ref idx);
        }
        private bool _ask_user(ref bool hasHeader, string[] colNames, dynamic[,] table, dynamic[][] thirdRow, ref short[] idx)
        {
            string[] cdn = getColumnDisplayNames();

            //ask for first row is heading or value due to obscurity,
            //in fact, if first row contains non-string value, it should be obvious (!obscure)
            if (hasHeader)
                hasHeader = askFirstColumn(table);

            return askLackColumn(cdn, table.CopyRow(0), ref idx);
        }

    }
}
