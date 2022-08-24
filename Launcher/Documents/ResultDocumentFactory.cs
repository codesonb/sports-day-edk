using EDKv5;
using EDKv5.MonitorServices;
using EDKv5.SchedulerService;
using Launcher.Wrappers;
using Launcher.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using System.Windows.Markup;
using System.Xml;

namespace Launcher.Documents
{
    static class ResultDocumentFactory
    {
        // const
        static readonly Thickness cellPadding = new Thickness(10, 5, 10, 5);
        static readonly Thickness cellBorder = new Thickness(0.5);

        public static FlowDocument CreateResultDocument(ICompetition[] competitions)
        {
            var doc = new FlowDocument()
            {
                FontFamily = new FontFamily("Arial"),
            };
            _create_competition_results_doc_page(doc, competitions);
            return doc;
        }

        private static void _create_competition_results_doc_page(FlowDocument doc, ICompetition[] competitions)
        {
            var project = Project.GetInstance();

            bool showLane = competitions[0].Event.NeedLaneAssignment;

            var eventName = string.Format("{0}\n{1}/{2}", project.Name, competitions[0].Group.GetName(), competitions[0].Event.Name);

            string titleFormat;
            if (showLane)
                titleFormat = string.Format("{0} - Heat {{0}} (of {1}) Competition Result", eventName, competitions.Length);
            else
                titleFormat = string.Format("{0} - Competition Result", eventName);

            for (int i = 0; i < competitions.Length; i++)
            {
                var competition = competitions[i];
                int heat = i + 1;

                // create head table
                var title = string.Format(titleFormat, heat);
                var section = new Section();
                section.BreakPageBefore = true;
                doc.Blocks.Add(section);

                _create_competition_results_doc_page(section, title, competition.Results, showLane);
            }

            //-- also add overall results if 
            if (competitions.Length > 1)
            {
                // merge result lists
                List<CompetitionResult> ls_allResults = new List<CompetitionResult>();
                foreach (var competition in competitions)
                {
                    foreach (var result in competition.Results)
                        if (0 != result.Value) ls_allResults.Add((CompetitionResult)result.Clone());
                }

                //-- sort
                ls_allResults.Sort();
                CompetitionResult.ComputeRanks(ls_allResults);
                int rnkCnt = 0;
                foreach (var result in ls_allResults)
                {
                    if (result.ComputedRank > 8) { break; }
                    result.Rank = result.ComputedRank;      // copy computed rank
                    rnkCnt++;
                }

                //-- print
                var overallTitle = string.Format("{0} - Overall Result", eventName);
                var arr_allResults = new CompetitionResult[rnkCnt];
                ls_allResults.CopyTo(0, arr_allResults, 0, rnkCnt);

                var section = new Section();
                section.BreakPageBefore = true;
                doc.Blocks.Add(section);
                _create_competition_results_doc_page(section, overallTitle, arr_allResults, false);
            }
        }
        private static void _create_competition_results_doc_page(Section section, string title, CompetitionResult[] results, bool showLane)
        {
            var titleParagraph = new Paragraph(new Underline(new Bold(new Run(title))))
            {
                FontSize = 24,
                TextAlignment = TextAlignment.Center,
            };
            section.Blocks.Add(titleParagraph);

            // create result table
            var resultTable = new Table()
            {
                FontSize = 14,
            };
            resultTable.CellSpacing = 0;
            resultTable.BorderBrush = Brushes.Black;
            resultTable.BorderThickness = new Thickness(1);

            // -- table headers
            var headerRowGroup = new TableRowGroup();
            var headerRow = new TableRow();

            if (showLane)
            {
                var laneHeader = new Paragraph(new Bold(new Run("Lane")));
                headerRow.Cells.Add(_create_TableCell(laneHeader));
            }

            var houseHeader = new Paragraph(new Bold(new Run("House")));
            var nameHeader = new Paragraph(new Bold(new Run("Athlet")));
            var resultHeader = new Paragraph(new Bold(new Run("Result")));
            var rankHeader = new Paragraph(new Bold(new Run("Rank")));

            headerRow.Cells.Add(_create_TableCell(houseHeader));
            headerRow.Cells.Add(_create_TableCell(nameHeader));
            headerRow.Cells.Add(_create_TableCell(resultHeader));
            headerRow.Cells.Add(_create_TableCell(rankHeader));

            headerRowGroup.Rows.Add(headerRow);
            resultTable.RowGroups.Add(headerRowGroup);

            // -- table content
            var resultRowGroup = new TableRowGroup();
            foreach (var result in results)
            {
                var resultRow = new TableRow();

                if (showLane)
                {
                    var laneCol = new Paragraph(new Bold(new Run(result.Lane.ToString())));
                    resultRow.Cells.Add(_create_TableCell(laneCol));
                }

                var houseCol = new Paragraph(new Bold(new Run(result.Participant.House.Name)));
                var nameCol = new Paragraph(new Bold(new Run(result.Participant.Name)));
                var resultCol = new Paragraph(new Bold(new Run(result.Result)));
                var rankCol = new Paragraph(new Bold(new Run(result.Rank.ToString())));

                resultRow.Cells.Add(_create_TableCell(houseCol));
                resultRow.Cells.Add(_create_TableCell(nameCol));
                resultRow.Cells.Add(_create_TableCell(resultCol));
                resultRow.Cells.Add(_create_TableCell(rankCol));

                resultRowGroup.Rows.Add(resultRow);
            }
            resultTable.RowGroups.Add(resultRowGroup);
            section.Blocks.Add(resultTable);

        } // end void _create_competition_results_doc_page

        private static TableCell _create_TableCell(Block block)
        {
            return new TableCell(block) { Padding = cellPadding, BorderThickness = cellBorder, BorderBrush = Brushes.Black };
        }


    } // end class
}
