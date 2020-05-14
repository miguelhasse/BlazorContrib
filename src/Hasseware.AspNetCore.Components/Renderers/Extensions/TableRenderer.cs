using System;
using System.Globalization;
using System.Linq;
using Markdig.Extensions.Tables;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class TableRenderer : BlazorObjectRenderer<Table>
    {
        protected override void Write(BlazorRenderer renderer, Table table)
        {
            renderer.OpenElement("table");

            if (table.ColumnDefinitions.Any(cd => cd.Width > 0 && cd.Width < 1))
            {
                foreach (var tableColumnDefinition in table.ColumnDefinitions)
                {
                    var width = Math.Round(tableColumnDefinition.Width * 100) / 100;
                    var widthValue = string.Format(CultureInfo.InvariantCulture, "{0:0.##}", width);
                    renderer.OpenElement("col");
                    renderer.AddAttribute("style", $"style=\"width:{widthValue}%\"");
                    renderer.CloseElement();
                }
            }

            bool bodyWritten = false;
            bool headerWritten = false;

            foreach (var rowObj in table)
            {
                var row = (TableRow)rowObj;

                if (row.IsHeader)
                {
                    if (!headerWritten)
                    {
                        renderer.OpenElement("thead");
                        headerWritten = true;
                    }
                }
                else if (!bodyWritten)
                {
                    if (headerWritten)
                    {
                        renderer.CloseElement();
                    }

                    renderer.OpenElement("tbody");
                    bodyWritten = true;
                }

                renderer.OpenElement("tr");
                renderer.WriteAttributes(row);

                for (int i = 0; i < row.Count; i++)
                {
                    var cell = (TableCell)row[i];
                    renderer.OpenElement(row.IsHeader ? "th" : "td");

                    if (cell.ColumnSpan != 1)
                    {
                        renderer.AddAttribute("colspan", cell.ColumnSpan);
                    }

                    if (cell.RowSpan != 1)
                    {
                        renderer.AddAttribute("rowspan", cell.RowSpan);
                    }

                    if (table.ColumnDefinitions.Count > 0)
                    {
                        var columnIndex = cell.ColumnIndex < 0 || cell.ColumnIndex >= table.ColumnDefinitions.Count ? i : cell.ColumnIndex;
                        columnIndex = columnIndex >= table.ColumnDefinitions.Count ? table.ColumnDefinitions.Count - 1 : columnIndex;

                        var alignment = table.ColumnDefinitions[columnIndex].Alignment;

                        if (alignment.HasValue)
                        {
                            switch (alignment)
                            {
                                case TableColumnAlign.Center:
                                    renderer.AddAttribute("style", "text-align:center;");
                                    break;
                                case TableColumnAlign.Left:
                                    renderer.AddAttribute("style", "text-align:left;");
                                    break;
                                case TableColumnAlign.Right:
                                    renderer.AddAttribute("style", "text-align:right;");
                                    break;
                            }
                        }

                        renderer.WriteAttributes(cell);
                        renderer.Write(cell);
                    }

                    renderer.CloseElement();
                }

                renderer.CloseElement();
            }

            if (bodyWritten)
            {
                renderer.CloseElement();
            }

            renderer.CloseElement();
        }
    }
}
