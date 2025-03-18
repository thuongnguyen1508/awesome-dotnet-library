using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TN.Excel.Exporting
{
    //
    // Summary:
    //     OpenXML convert helper
    public static class OpenXmlConvert
    {
        //
        // Summary:
        //     To get the row index from cell name or cell reference number
        //
        // Parameters:
        //   cellReference:
        //     Cell address
        //
        // Returns:
        //     The specific row index
        internal static int GetRowIndex(string cellReference)
        {
            return int.Parse(new Regex("\\d+").Match(cellReference).Value, CultureInfo.InvariantCulture);
        }

        //
        // Summary:
        //     Convert a zero-based column index into an Excel column reference
        //
        // Parameters:
        //   columnNumber:
        //     Column index
        //
        // Returns:
        //     The specific column address
        internal static string ToExcelColumnName(int columnNumber)
        {
            string text = ((columnNumber == 0) ? "A" : string.Empty);
            while (columnNumber > 0)
            {
                int num = columnNumber % 26;
                text = Convert.ToChar(65 + num) + text;
                columnNumber = (columnNumber - num) / 26;
            }

            return text;
        }

        //
        // Summary:
        //     Generate cell from Property Info and row data
        //
        // Parameters:
        //   propertyInfo:
        //     Property Info
        //
        //   rowData:
        //     Row data
        //
        // Type parameters:
        //   TTRowData:
        //     Type of list row data
        //
        // Returns:
        //     The specific cell
        internal static Cell ToCell<TTRowData>(PropertyInfo propertyInfo, TTRowData rowData)
        {
            object value = propertyInfo.GetValue(rowData);
            Cell cell = new Cell();
            if (!(value is short) && !(value is int) && !(value is long) && !(value is ushort) && !(value is uint) && !(value is ulong) && !(value is byte) && !(value is sbyte) && !(value is double) && !(value is decimal) && !(value is float))
            {
                if (!(value is DateTime dateTime))
                {
                    if (value != null)
                    {
                        cell.StyleIndex = 0u;
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(value.ToString());
                    }
                }
                else
                {
                    cell.StyleIndex = 1u;
                    cell.DataType = CellValues.Number;
                    cell.CellValue = new CellValue(dateTime.ToOADate().ToString(CultureInfo.CurrentCulture));
                }
            }
            else
            {
                cell.StyleIndex = 0u;
                cell.DataType = CellValues.Number;
                cell.CellValue = new CellValue(value.ToString());
            }

            return cell;
        }
    }
}
