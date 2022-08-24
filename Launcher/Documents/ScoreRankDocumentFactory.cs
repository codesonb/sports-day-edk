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

using Launcher.Algorithms;

namespace Launcher.Documents
{
    static class ScoreRankDocumentFactory
    {
        // const
        static readonly Thickness cellPadding = new Thickness(10, 5, 10, 5);
        static readonly Thickness cellBorder = new Thickness(0.5);

        public static FlowDocument CreateScoreRankDocument<T>(string documentHeader, string winnerHeader, Score<T>[] scores, Func<T, string> dlg_getKeyName)
        {
            var project = Project.GetInstance();

            // create document skeleton
            var doc = new FlowDocument()
            {
                FontFamily = new FontFamily("Arial"),
            };
            var section = new Section();

            // create title
            var title = string.Format( "{0}\n{1}", project.Name, documentHeader );
            var titleParagraph = new Paragraph(new Underline(new Bold(new Run(title))))
            {
                FontSize = 24,
                TextAlignment = TextAlignment.Center,
            };
            section.Blocks.Add(titleParagraph);

            // 
            _create_score_ranks_doc_page(section, winnerHeader, scores, dlg_getKeyName);

            // close document
            doc.Blocks.Add(section);
            return doc;
        }
        private static void _create_score_ranks_doc_page<T>(Section section, string winnerHeaderName, Score<T>[] scores, Func<T, string> dlg_getKeyName)
        {
            // create result table
            var resultTable = new Table()
            {
                FontSize = 14,
            };
            resultTable.CellSpacing = 0;
            resultTable.BorderBrush = Brushes.Black;
            resultTable.BorderThickness = new Thickness(1);

            //-- table columns
            var rankColumn = new TableColumn() { Width = new GridLength(60d) };
            var winnerColumn = new TableColumn() { Width = new GridLength(150d) };
            var participationColumn = new TableColumn();
            var rankScoreColumn = new TableColumn();
            var dedcutColumn = new TableColumn();
            var totalColumn = new TableColumn();

            resultTable.Columns.Add(rankColumn);
            resultTable.Columns.Add(winnerColumn);
            resultTable.Columns.Add(participationColumn);
            resultTable.Columns.Add(rankScoreColumn);
            resultTable.Columns.Add(dedcutColumn);
            resultTable.Columns.Add(totalColumn);

            // -- table headers
            var headerRowGroup = new TableRowGroup();
            var headerRow = new TableRow();

            var rankHeader = new Paragraph(new Bold(new Run(Properties.Resources.strRank)));
            var winnerHeader = new Paragraph(new Bold(new Run(winnerHeaderName)));
            var participationHeader = new Paragraph(new Bold(new Run(Properties.Resources.strParticipationScore)));
            var rankScoreHeader = new Paragraph(new Bold(new Run(Properties.Resources.strRankScore)));
            var deductionHeader = new Paragraph(new Bold(new Run(Properties.Resources.strDeduction)));
            var totalHeader = new Paragraph(new Bold(new Run(Properties.Resources.strTotalScore)));

            headerRow.Cells.Add(_create_TableCell(rankHeader));
            headerRow.Cells.Add(_create_TableCell(winnerHeader));
            headerRow.Cells.Add(_create_TableCell(participationHeader));
            headerRow.Cells.Add(_create_TableCell(rankScoreHeader));
            headerRow.Cells.Add(_create_TableCell(deductionHeader));
            headerRow.Cells.Add(_create_TableCell(totalHeader));

            headerRowGroup.Rows.Add(headerRow);
            resultTable.RowGroups.Add(headerRowGroup);

            // -- table content
            var resultRowGroup = new TableRowGroup();
            foreach (var result in scores)
            {
                var resultRow = new TableRow();

                var score = result.Value;

                var rankCol = new Paragraph(new Bold(new Run(result.Rank.ToString())));
                var winnerCol = new Paragraph(new Bold(new Run(dlg_getKeyName(result.Key))));
                var participationCol = new Paragraph(new Bold(new Run(score.Participation.ToString())));
                var rankScoreCol = new Paragraph(new Bold(new Run((score.RankScore + score.RelayDoubled).ToString())));
                var deductionCol = new Paragraph(new Bold(new Run((-score.Deduction).ToString())));
                var totalCol = new Paragraph(new Bold(new Run(score.Total.ToString())));

                resultRow.Cells.Add(_create_TableCell(rankCol));
                resultRow.Cells.Add(_create_TableCell(winnerCol));
                resultRow.Cells.Add(_create_TableCell(participationCol));
                resultRow.Cells.Add(_create_TableCell(rankScoreCol));
                resultRow.Cells.Add(_create_TableCell(deductionCol));
                resultRow.Cells.Add(_create_TableCell(totalCol));

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
