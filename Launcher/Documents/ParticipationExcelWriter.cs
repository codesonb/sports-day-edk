using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using EDKv5;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Launcher
{
    static class ParticipationExcelWriter
    {
        public static void Write(Project project, Stream stream)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add(project.Name);

                int curRow = 1;
                foreach (var ev in project.Events)
                {
                    foreach (var kvp in ev.Competitions)
                    {
                        ws.Cells[curRow, 1].Value = ev.Name;
                        ws.Cells[curRow, 2].Value = kvp.Item1.GetName();
                        ws.Cells[curRow, 1, curRow, 17].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        curRow++;

                        if (ev.NeedLaneAssignment)
                        {
                            // -- lane name
                            for (int i = 1; i < 9; i++)
                            {
                                var colIdx = i * 2;
                                var range = ws.Cells[curRow, colIdx, curRow, colIdx + 1];
                                range.Merge = true;
                                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                range.Value = "Lane " + i;
                            }
                            curRow++;

                            // participants
                            int heat = 0;
                            foreach (var cmp in kvp.Item2)
                            {
                                var heatNoCell = ws.Cells[curRow, 1];
                                heatNoCell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                heatNoCell.Value = "Heat " + (++heat);
                                int curCol = 0;
                                foreach (var ppt in cmp.Participants)
                                {
                                    curCol += 2;

                                    ws.Cells[curRow, curCol].Value = ppt.House.Key.ToString();
                                    ws.Cells[curRow, curCol + 1].Value =  string.Format("{0} ({1})", ppt.Name, ppt.ClassName);

                                    var range = ws.Cells[curRow, curCol, curRow, curCol + 1];
                                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                }
                                while (curCol < 16)
                                {
                                    curCol += 2;
                                    var range = ws.Cells[curRow, curCol, curRow, curCol + 1];
                                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                }
                                curRow++;
                            }
                        }
                        else
                        {
                            // enumerate participants
                            int curCol = 0;
                            foreach (var ppt in kvp.Item2[0].Participants)
                            {
                                curCol += 2;
                                ws.Cells[curRow, curCol].Value = ppt.House.Key.ToString();
                                ws.Cells[curRow, curCol + 1].Value = string.Format("{0} ({1})", ppt.Name, ppt.ClassName);

                                var range = ws.Cells[curRow, curCol, curRow, curCol + 1];
                                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                                // move next row
                                if (curCol > 15) { curCol = 0; curRow++; }
                            }
                            while (curCol < 16)
                            {
                                curCol += 2;
                                var range = ws.Cells[curRow, curCol, curRow, curCol + 1];
                                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            }
                        }
                        curRow++;

                    } // for each competition
                } // for each event

                package.SaveAs(stream);

            }
        } // end void Write
    }
}
