﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using BattaTech.ExcelExport.Entities;
using System.Data;
using System.Drawing;

namespace BattaTech.ExcelExport
{
    public class GridExport
    {
        public static void Export(ExcelSheet excel)
        {
            if (excel != null && !string.IsNullOrEmpty(excel.fileName) && excel.grids != null && excel.grids.Count > 0 &&
                !string.IsNullOrEmpty(excel.author) && !string.IsNullOrEmpty(excel.company) && !string.IsNullOrEmpty(excel.version))
            {
                StringBuilder result = new StringBuilder();

                #region Info Section

                result.Append("<?xml version=\"1.0\"?>");
                result.Append("<?mso-application progid=\"Excel.Sheet\"?>");
                result.Append("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                result.Append(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
                result.Append(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
                result.Append(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                result.Append(" xmlns:html=\"http://www.w3.org/TR/REC-html40/\">");
                result.Append(" <DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">");
                result.Append(string.Concat("  <Author>", excel.author, "</Author>"));
                result.Append(string.Format("  <Created>{0}T{1}Z</Created>", DateTime.Now.ToString("yyyy-mm-dd"), DateTime.Now.ToString("HH:MM:SS")));
                result.Append(string.Concat("  <Company>", excel.company, "</Company>"));
                result.Append(string.Concat("  <Version>", excel.version, "</Version>"));
                result.Append(" </DocumentProperties>");
                result.Append("<OfficeDocumentSettings xmlns=\"urn:schemas-microsoft-com:office:office\">");
                result.Append("<AllowPNG/>");
                result.Append("</OfficeDocumentSettings>");
                result.Append(" <ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">");
                result.Append("  <WindowHeight>8955</WindowHeight>");
                result.Append("  <WindowWidth>11355</WindowWidth>");
                result.Append("  <WindowTopX>480</WindowTopX>");
                result.Append("  <WindowTopY>15</WindowTopY>");
                result.Append("  <ProtectStructure>False</ProtectStructure>");
                result.Append("  <ProtectWindows>False</ProtectWindows>");
                result.Append(" </ExcelWorkbook>");
                result.Append(" <Styles>");
                result.Append("  <Style ss:ID=\"Default\" ss:Name=\"Normal\">");
                result.Append("   <Alignment ss:Vertical=\"Bottom\"/>");
                result.Append("   <Borders/>");
                result.Append("   <Font ss:FontName=\"Arial\"/>");
                result.Append("   <Interior/>");
                result.Append("   <Protection/>");
                result.Append("  </Style>");

                #endregion Info Section

                #region Date & Format Styling Section

                int styleId = excel.defaultStyleId;
                List<ExcelStyle> alreadyAdded = new List<ExcelStyle>();

                result.Append("<Style ss:ID=\"s" + styleId.ToString() + "\">");
                result.Append("<Alignment ss:Vertical=\"Bottom\" ss:WrapText=\"1\"/>");
                result.Append("</Style>");
                styleId++;

                foreach (Grid grid in excel.grids)
                {
                    result.Append(string.Concat("<Style ss:ID=\"s", styleId, "\">"));
                    result.Append("<Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Bottom\" ss:WrapText=\"1\"/>");
                    result.Append(string.Concat("<Font ss:FontName=\"Arial\" ss:Color=\"", grid.headerForeColor, "\" ss:Bold=\"1\"/>"));
                    result.Append(string.Concat("<Interior ss:Color=\"", grid.headerBackgroundColor, "\" ss:Pattern=\"Solid\"/>"));
                    result.Append("</Style>");

                    grid.headerStyleId = styleId;
                    styleId++;

                    if (grid.dataTable != null && grid.dataTable.Columns != null && grid.dataTable.Columns.Count > 0)
                    {
                        foreach (DataColumn column in grid.dataTable.Columns)
                        {
                            if (grid.columnsConfiguration != null && grid.columnsConfiguration.Count > 0)
                            {
                                ColumnModel columnConfig = grid.columnsConfiguration.Find(x => x.columnName == column.ColumnName);

                                if (columnConfig != null && !columnConfig.isHidden)
                                {
                                    var alreadyAddedStyle = alreadyAdded.Find(x => x.dataType == columnConfig.style.dataType && x.dataFormat == columnConfig.style.dataFormat);

                                    if (alreadyAddedStyle != null)
                                    {
                                        columnConfig.style.dataFormatStyleId = alreadyAddedStyle.dataFormatStyleId;
                                    }
                                    else
                                    {
                                        if (columnConfig.style.dataType == typeof(int))
                                        {
                                            result.Append("<Style ss:ID=\"s" + styleId.ToString() + "\">");
                                            result.Append("<NumberFormat ss:Format=\"0\"/>");
                                            result.Append("<Alignment ss:Vertical=\"Bottom\" ss:WrapText=\"1\"/>");
                                            result.Append("</Style>");

                                            columnConfig.style.dataFormatStyleId = styleId;
                                            alreadyAdded.Add(columnConfig.style);

                                            styleId++;
                                        }
                                        else if (columnConfig.style.dataType == typeof(float) || columnConfig.style.dataType == typeof(double))
                                        {
                                            result.Append("<Style ss:ID=\"s" + styleId.ToString() + "\">");
                                            result.Append("<NumberFormat ss:Format=\"Fixed\"/>");
                                            result.Append("<Alignment ss:Vertical=\"Bottom\" ss:WrapText=\"1\"/>");
                                            result.Append("</Style>");

                                            columnConfig.style.dataFormatStyleId = styleId;
                                            alreadyAdded.Add(columnConfig.style);

                                            styleId++;
                                        }
                                        else if (columnConfig.style.dataType == typeof(DateTime))
                                        {
                                            if (string.IsNullOrEmpty(columnConfig.style.dataFormat))
                                            {
                                                columnConfig.style.dataFormat = "dd-MMM-yyyy";
                                            }

                                            result.Append("<Style ss:ID=\"s" + styleId.ToString() + "\">");
                                            result.Append("<NumberFormat ss:Format=\"[ENG][$-409]" + GetExcelCompliantDataFormat(columnConfig.style.dataFormat) + ";@\"/>");
                                            result.Append("<Alignment ss:Vertical=\"Bottom\" ss:WrapText=\"1\"/>");
                                            result.Append("</Style>");

                                            columnConfig.style.dataFormatStyleId = styleId;
                                            alreadyAdded.Add(columnConfig.style);

                                            styleId++;
                                        }
                                        else
                                        {
                                            columnConfig.style.dataFormatStyleId = excel.defaultStyleId;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion Date & Format Styling Section

                result.Append(string.Concat("  <Style ss:ID=\"s", styleId, "\">"));
                result.Append("   <Alignment ss:Vertical=\"Bottom\" ss:WrapText=\"1\"/>");
                result.Append("  </Style>");
                result.Append(" </Styles>");

                styleId++;

                int workSheetNumber = 1;
                bool firstGrid = true;

                foreach (Grid grid in excel.grids)
                {
                    if (grid.dataTable != null && grid.dataTable.Rows != null && grid.dataTable.Rows.Count > 0)
                    {
                        #region Setting Sheet Name

                        if (!string.IsNullOrEmpty(grid.tableName))
                        {
                            result.Append(string.Concat(" <Worksheet ss:Name=\"", grid.tableName, "\">"));
                        }
                        else if (!string.IsNullOrEmpty(grid.dataTable.TableName))
                        {
                            result.Append(string.Concat(" <Worksheet ss:Name=\"", grid.dataTable.TableName, "\">"));
                        }
                        else
                        {
                            result.Append(string.Concat(" <Worksheet ss:Name=\"Sheet-", workSheetNumber.ToString(), "\">"));
                        }

                        #endregion Setting Sheet Name

                        result.Append(string.Format("  <Table ss:ExpandedColumnCount=\"{0}\" ss:ExpandedRowCount=\"{1}\" x:FullColumns=\"1\"", grid.dataTable.Columns.Count, (grid.dataTable.Rows.Count + 1)));
                        result.Append("     x:FullRows=\"1\">");

                        #region Setting Column Width

                        foreach (ColumnModel columnConfig in grid.columnsConfiguration)
                        {
                            if (!string.IsNullOrEmpty(columnConfig.columnName) && !columnConfig.isHidden)
                            {
                                if (grid.dataTable.Columns[columnConfig.columnName] != null)
                                {
                                    result.Append(string.Concat("<Column ss:AutoFitWidth=\"0\" ss:Width=\"", columnConfig.columnWidth, "\" />"));
                                }
                            }
                        }

                        #endregion Setting Column Width

                        #region Creating Header Row

                        result.Append("<Row>");

                        foreach (ColumnModel columnConfig in grid.columnsConfiguration)
                        {
                            if (!string.IsNullOrEmpty(columnConfig.columnName) && !columnConfig.isHidden)
                            {
                                DataColumn column = grid.dataTable.Columns[columnConfig.columnName];

                                if (column != null)
                                {
                                    result.Append(string.Concat("<Cell ss:StyleID=\"s", grid.headerStyleId, "\"><Data ss:Type=\"String\">", columnConfig.headerText, "</Data></Cell>"));
                                }
                            }
                        }

                        result.Append("</Row>");

                        #endregion Creating Header Row

                        foreach (DataRow row in grid.dataTable.Rows)
                        {
                            result.Append("<Row>");

                            foreach (ColumnModel columnConfig in grid.columnsConfiguration)
                            {
                                if (!string.IsNullOrEmpty(columnConfig.columnName) && !columnConfig.isHidden)
                                {
                                    DataColumn column = grid.dataTable.Columns[columnConfig.columnName];

                                    if (column != null)
                                    {
                                        string data = Convert.ToString(row[column]);
                                        int dataFormatStyleId = 0;
                                        string type = string.Empty;

                                        if (columnConfig.style.dataFormatStyleId > 0)
                                        {
                                            dataFormatStyleId = columnConfig.style.dataFormatStyleId;

                                            if (columnConfig.style.dataType == typeof(int) || columnConfig.style.dataType == typeof(float) || columnConfig.style.dataType == typeof(double))
                                            {
                                                type = "Number";
                                            }
                                            else if (columnConfig.style.dataType == typeof(DateTime))
                                            {
                                                type = "DateTime";
                                                data = GetExcelCompliantDateFormat((DateTime)row[column]);
                                            }
                                            else
                                            {
                                                type = "String";
                                            }
                                        }

                                        result.Append(string.Concat("<Cell ss:StyleID=\"s", dataFormatStyleId, "\">"));
                                        result.Append(string.Concat("<Data ss:Type=\"", type, "\">", data, "</Data>"));
                                        result.Append("</Cell>");
                                    }
                                }
                            }

                            result.Append("</Row>");
                        }

                        result.Append("  </Table>");
                        result.Append("  <WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">");

                        if (firstGrid)
                        {
                            result.Append("<Print>");
                            result.Append("<ValidPrinterInfo/>");
                            result.Append("<HorizontalResolution>600</HorizontalResolution>");
                            result.Append("<VerticalResolution>600</VerticalResolution>");
                            result.Append("</Print>");
                            result.Append("<Selected/>");
                        }

                        result.Append("   <Panes>");
                        result.Append("    <Pane>");
                        result.Append("     <Number>3</Number>");
                        result.Append("     <ActiveRow>1</ActiveRow>");

                        if (firstGrid)
                        {
                            result.Append("     <ActiveCol>1</ActiveCol>");
                        }

                        result.Append("    </Pane>");
                        result.Append("   </Panes>");
                        result.Append("   <ProtectObjects>False</ProtectObjects>");
                        result.Append("   <ProtectScenarios>False</ProtectScenarios>");
                        result.Append("  </WorksheetOptions>");
                        result.Append(" </Worksheet>");
                    }

                    if (firstGrid)
                    {
                        firstGrid = false;
                    }

                    workSheetNumber++;
                }

                result.Append("</Workbook>");

                // Export to excel
                GenerateExcel(excel.fileName, result.ToString());
            }
        }

        private static void GenerateExcel(string fileName, string table)
        {
            if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(table))
            {
                HttpContext.Current.Response.Clear();

                HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".xls");
                HttpContext.Current.Response.ContentType = "application/ms-excel";

                HttpContext.Current.Response.Write(table);
                HttpContext.Current.Response.End();
            }
        }

        private static string GetExcelCompliantDataFormat(string input)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(input))
            {
                result = input.Replace("-", "\\-").Replace("/", "\\/").Replace(" ", "\\ ").Replace(",", "\\,");
            }

            return result;
        }

        private static string GetExcelCompliantDateFormat(DateTime dateTime)
        {
            string result = string.Empty;

            string date = dateTime.ToString("yyyy-MM-dd");
            string time = dateTime.ToString("HH:mm:ss");
            result = date + "T" + time + ".000";

            return result;
        }

        /*
        private static Table GetTablularData(GridView gridView, int[] removeColumns, List<string> customHeaders)
        {
            Table table = new Table();

            if (gridView != null && gridView.Rows.Count > 0)
            {
                if (gridView.AllowPaging == true)
                {
                    gridView.AllowPaging = false;
                    gridView.DataBind();
                }

                if (removeColumns != null && removeColumns.Length > 0)
                {
                    Array.Sort<int>(removeColumns, new Comparison<int>((i1, i2) => i2.CompareTo(i1)));
                }

                table.BorderColor = System.Drawing.Color.Black;
                table.BorderWidth = Unit.Pixel(1);

                if (customHeaders != null && customHeaders.Count > 0)
                {
                    using (TableHeaderRow thr = new TableHeaderRow())
                    {
                        foreach (string item in customHeaders)
                        {
                            TableCell tc = new TableCell();

                            tc.BackColor = System.Drawing.Color.Black;
                            tc.BorderColor = System.Drawing.Color.Black;
                            tc.BorderWidth = Unit.Pixel(1);
                            tc.ForeColor = System.Drawing.Color.White;
                            tc.Text = item;

                            thr.Cells.Add(tc);
                        }

                        table.Rows.Add(thr);
                    }
                }

                //  add the header row to the table
                if (gridView.HeaderRow != null)
                {
                    GridExport.PrepareControlForExport(gridView.HeaderRow);
                    GridViewRow gr = gridView.HeaderRow;

                    if (removeColumns != null && removeColumns.Length > 0)
                    {
                        foreach (var item in removeColumns)
                        {
                            if (item <= gridView.Columns.Count)
                            {
                                gr.Cells.RemoveAt(item);
                            }
                        }
                    }

                    for (int i = 0; i < gr.Cells.Count; i++)
                    {
                        using (TableHeaderRow tr = new TableHeaderRow())
                        {
                            while (gr.Cells.Count > 0)
                            {
                                using (TableCell tc = gr.Cells[0])
                                {
                                    tc.BackColor = System.Drawing.Color.Black;
                                    tc.BorderColor = System.Drawing.Color.Black;
                                    tc.BorderWidth = Unit.Pixel(1);
                                    tc.ForeColor = System.Drawing.Color.White;

                                    tr.Cells.Add(tc);
                                }
                            }

                            table.Rows.Add(tr);
                        }
                    }
                }

                //  add each of the data rows to the table
                foreach (GridViewRow row in gridView.Rows)
                {
                    GridExport.PrepareControlForExport(row);
                    GridViewRow gr = row;

                    if (removeColumns != null && removeColumns.Length > 0)
                    {
                        foreach (var item in removeColumns)
                        {
                            if (item <= gridView.Columns.Count)
                            {
                                gr.Cells.RemoveAt(item);
                            }
                        }
                    }

                    for (int i = 0; i < gr.Cells.Count; i++)
                    {
                        using (TableRow tr = new TableRow())
                        {
                            while (gr.Cells.Count > 0)
                            {
                                using (TableCell tc = gr.Cells[0])
                                {
                                    tc.BorderColor = System.Drawing.Color.Black;
                                    tc.BorderWidth = Unit.Pixel(1);

                                    tr.Cells.Add(tc);
                                }
                            }

                            table.Rows.Add(tr);
                        }
                    }
                }

                //  add the footer row to the table
                if (gridView.FooterRow != null)
                {
                    GridExport.PrepareControlForExport(gridView.FooterRow);
                    GridViewRow gr = gridView.FooterRow;

                    if (removeColumns != null && removeColumns.Length > 0)
                    {
                        foreach (var item in removeColumns)
                        {
                            if (item <= gridView.Columns.Count)
                            {
                                gr.Cells.RemoveAt(item);
                            }
                        }
                    }

                    for (int i = 0; i < gr.Cells.Count; i++)
                    {
                        using (TableFooterRow tr = new TableFooterRow())
                        {
                            while (gr.Cells.Count > 0)
                            {
                                using (TableCell tc = gr.Cells[0])
                                {
                                    tc.BackColor = System.Drawing.Color.Black;
                                    tc.BorderColor = System.Drawing.Color.Black;
                                    tc.BorderWidth = Unit.Pixel(1);
                                    tc.ForeColor = System.Drawing.Color.White;

                                    tr.Cells.Add(tc);
                                }
                            }

                            table.Rows.Add(tr);
                        }
                    }
                }
            }

            return table;
        }

        private static void PrepareControlForExport(Control control)
        {
            for (int i = 0; i < control.Controls.Count; i++)
            {
                Control current = control.Controls[i];

                if (current is LinkButton)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as LinkButton).Text));
                }
                else if (current is Image)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as Image).AlternateText));
                }
                else if (current is ImageButton)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as ImageButton).AlternateText));
                }
                else if (current is HtmlImage)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as HtmlImage).Alt));
                }
                else if (current is HyperLink)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as HyperLink).Text));
                }
                else if (current is HtmlAnchor)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as HtmlAnchor).Title));
                }
                else if (current is DropDownList)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as DropDownList).SelectedItem.Text));
                }
                else if (current is CheckBox)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as CheckBox).Checked ? "True" : "False"));
                }

                if (current.HasControls())
                {
                    GridExport.PrepareControlForExport(current);
                }
            }
        }

        private static string GetStringData(Table table)
        {
            string result = string.Empty;

            if (table != null && table.Rows != null && table.Rows.Count > 0)
            {
                using (StringWriter sw = new StringWriter())
                {
                    using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                    {
                        //  render the table into the htmlwriter
                        table.RenderControl(htw);

                        //  render the htmlwriter into the response
                        result = sw.ToString();
                    }
                }
            }

            return result;
        }

        
        */
    }
}
