using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Globalization;
using System.Reflection;
using TN.Excel.Attributes;

namespace TN.Excel.Exporting
{
    //
    // Summary:
    //     OpenXML action helper
    internal static class OpenXmlAction
    {
        private const string HeaderCellReferenceFormat = "{0}1";

        //
        // Summary:
        //     Append a list TAppendData into sheet data based on property info as column name
        //
        //
        // Parameters:
        //   sheetData:
        //     OpenXmlElement
        //
        //   exportData:
        //     List data need to append
        //
        //   propertyInfos:
        //     List of property info
        //
        // Type parameters:
        //   TAppendData:
        //     Type of list append data
        internal static void AppendRange<TAppendData>(this OpenXmlElement sheetData, List<TAppendData> exportData, List<PropertyInfo> propertyInfos)
        {
            exportData.ForEach(delegate (TAppendData rowData)
            {
                Row childRow = new Row
                {
                    RowIndex = (uint)(exportData.IndexOf(rowData) + 2)
                };
                propertyInfos.ForEach(delegate (PropertyInfo propertyInfo)
                {
                    Cell cell = OpenXmlConvert.ToCell(propertyInfo, rowData);
                    cell.CellReference = $"{OpenXmlConvert.ToExcelColumnName(propertyInfos.IndexOf(propertyInfo))}{exportData.IndexOf(rowData) + 2}";
                    childRow.AppendChild(cell);
                });
                sheetData.AppendChild(childRow);
            });
        }

        //
        // Summary:
        //     Append header name based on list properties info
        //
        // Parameters:
        //   sheetData:
        //     Sheet data
        //
        //   propertiesInfos:
        //     List of property info
        internal static void AppendHeaderNames(OpenXmlElement sheetData, List<PropertyInfo> propertiesInfos)
        {
            Row headerRow = new Row
            {
                RowIndex = 1u
            };
            propertiesInfos.ForEach(delegate (PropertyInfo propertyInfo)
            {
                string text = propertyInfo.GetCustomAttribute<ExcelColumnHeaderAttribute>()?.Name;
                if (text != null)
                {
                    headerRow.AppendChild(new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(text),
                        CellReference = string.Format(CultureInfo.CurrentCulture, "{0}1", OpenXmlConvert.ToExcelColumnName(propertiesInfos.IndexOf(propertyInfo)))
                    });
                }
            });
            sheetData.AppendChild(headerRow);
        }

        //
        // Summary:
        //     Set value of cell whatever merged cell or not
        //
        // Parameters:
        //   cellAddress:
        //     Cell Address
        //
        //   textValue:
        //     Value
        //
        //   workbookPart:
        //     Work book part
        //
        //   sheetId:
        //     Id of sheet
        //
        // Exceptions:
        //   T:System.ArgumentOutOfRangeException:
        //     Exception throw when cell address is invalid or out of dimension
        internal static void AppendValueToCell(WorkbookPart workbookPart, StringValue sheetId, string cellAddress, string textValue)
        {
            Cell cell = ((WorksheetPart)workbookPart.GetPartById(sheetId)).Worksheet.GetFirstChild<SheetData>().Elements<Row>().FirstOrDefault((Row rowElement) => (uint)rowElement.RowIndex == OpenXmlConvert.GetRowIndex(cellAddress))?.Elements<Cell>().FirstOrDefault((Cell cellElement) => cellElement.CellReference.Value.Equals(cellAddress, StringComparison.OrdinalIgnoreCase));
            if (cell == null)
            {
                throw new ArgumentOutOfRangeException("cellAddress", ((WorksheetPart)workbookPart.GetPartById(sheetId)).Worksheet.SheetDimension.Reference, null);
            }

            cell.CellValue = new CellValue(textValue);
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                cell.CellValue = new CellValue(AppendText(workbookPart.SharedStringTablePart, textValue).ToString(CultureInfo.InvariantCulture));
                cell.DataType = CellValues.SharedString;
            }
        }

        //
        // Summary:
        //     Insert textValue to shared string table to make sure work well in case merged
        //     cell
        //
        // Parameters:
        //   shareStringPart:
        //     Shared String Table Part
        //
        //   textValue:
        //     Text value
        //
        // Returns:
        //     Index of text value just added in shared string table
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     Cannot save the shared string table
        private static int AppendText(SharedStringTablePart shareStringPart, string textValue)
        {
            if (shareStringPart.SharedStringTable == null)
            {
                SharedStringTable sharedStringTable2 = (shareStringPart.SharedStringTable = new SharedStringTable());
            }

            List<SharedStringItem> list = shareStringPart.SharedStringTable.Elements<SharedStringItem>().ToList();
            SharedStringItem sharedStringItem = list.FirstOrDefault((SharedStringItem item) => item.InnerText.Equals(textValue, StringComparison.InvariantCulture));
            if (sharedStringItem != null)
            {
                return list.IndexOf(sharedStringItem);
            }

            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new Text(textValue)));
            shareStringPart.SharedStringTable.Save();
            return list.Count;
        }
    }
}
