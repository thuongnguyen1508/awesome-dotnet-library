using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Text.RegularExpressions;
using TN.Excel.Exporting;
using System.Globalization;

namespace TN.Excel.Importing
{
    public static class OpenXmlImporter
    {
        private const char ColumnStartAddress = 'A';

        //
        // Summary:
        //     Read excel from file path and parse into class
        //
        // Parameters:
        //   filePath:
        //     Excel file file path
        //
        //   worksheetName:
        //     Worksheet name
        //
        //   containHeaderRow:
        //     Is that excel contain header
        //
        //   typeConverter:
        //     Dictionary type converter
        //
        // Type parameters:
        //   TExcelData:
        //     Type of class
        //
        // Returns:
        //     The specific list of TExcelData read from file path
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     Exception thrown when file Path is null
        //
        //   T:System.ArgumentNullException:
        //     Exception thrown when can't find sheet by giving work sheet name
        //
        //   T:System.IO.FileNotFoundException:
        //     Exception thrown when file excel not found
        //
        //   T:System.NotSupportedException:
        //     Exception thrown when excel property is required but value is null
        public static List<TExcelData> ReadExcel<TExcelData>(string filePath, string worksheetName, bool containHeaderRow, Dictionary<Type, Func<string, object>> typeConverter = null) where TExcelData : new()
        {
            using SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filePath, isEditable: true, new OpenSettings
            {
                //RelationshipErrorHandlerFactory = (OpenXmlPackage package) => new UriRelationshipErrorHandler()
            });
            StringValue stringValue = (from sheet in spreadsheetDocument.WorkbookPart.Workbook.Descendants<Sheet>()
                                       where sheet.Name == worksheetName
                                       select sheet.Id.Value).FirstOrDefault();
            if ((object)stringValue == null)
            {
                throw new ArgumentNullException("worksheetName");
            }

            Worksheet worksheet = ((WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(stringValue)).Worksheet;
            SharedStringTablePart sharedStringTablePart = spreadsheetDocument.WorkbookPart.SharedStringTablePart;
            return OpenXmlImportHelper.Parse<TExcelData>(GetExcelData(worksheet, sharedStringTablePart, containHeaderRow), typeConverter);
        }

        //
        // Summary:
        //     Read excel from file path and parse into class
        //
        // Parameters:
        //   stream:
        //     Excel file memoryStream
        //
        //   worksheetName:
        //     Worksheet name
        //
        //   containHeaderRow:
        //     Is that excel contain header
        //
        //   typeConverter:
        //     Dictionary type converter
        //
        // Type parameters:
        //   TExcelData:
        //     Type of class
        //
        // Returns:
        //     The specific list of TExcelData read from file path
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     Exception thrown when file Path is null
        //
        //   T:System.ArgumentNullException:
        //     Exception thrown when can't find sheet by giving work sheet name
        //
        //   T:System.IO.FileNotFoundException:
        //     Exception thrown when file excel not found
        //
        //   T:System.NotSupportedException:
        //     Exception thrown when excel property is required but value is null
        public static List<TExcelData> ReadExcelFromStream<TExcelData>(Stream stream, string worksheetName, bool containHeaderRow, Dictionary<Type, Func<string, object>> typeConverter = null) where TExcelData : new()
        {
            using SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(stream, isEditable: true, new OpenSettings
            {
                //RelationshipErrorHandlerFactory = (OpenXmlPackage package) => new UriRelationshipErrorHandler()
            });
            StringValue stringValue = (from sheet in spreadsheetDocument.WorkbookPart.Workbook.Descendants<Sheet>()
                                       where sheet.Name == worksheetName
                                       select sheet.Id.Value).FirstOrDefault();
            if ((object)stringValue == null)
            {
                throw new ArgumentNullException("worksheetName");
            }

            Worksheet worksheet = ((WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(stringValue)).Worksheet;
            SharedStringTablePart sharedStringTablePart = spreadsheetDocument.WorkbookPart.SharedStringTablePart;
            return OpenXmlImportHelper.Parse<TExcelData>(GetExcelData(worksheet, sharedStringTablePart, containHeaderRow), typeConverter);
        }

        private static string[,] GetExcelData(Worksheet worksheet, SharedStringTablePart stringTablePart, bool containHeaderRow)
        {
            SheetData firstChild = worksheet.GetFirstChild<SheetData>();
            IEnumerable<Row> source;
            if (!containHeaderRow)
            {
                IEnumerable<Row> enumerable = firstChild.Descendants<Row>();
                source = enumerable;
            }
            else
            {
                source = firstChild.Descendants<Row>().Skip(1);
            }

            List<Row> list = source.ToList();
            (int ColumnCount, int RowCount) tuple = CalculateExcelTableSize(worksheet, containHeaderRow);
            int item = tuple.ColumnCount;
            int item2 = tuple.RowCount;
            string[,] excelRawValue = new string[item, item2];
            list.ForEach(delegate (Row row)
            {
                row.Descendants<Cell>().ToList().ForEach(delegate (Cell cell)
                {
                    if (cell.CellReference != null && cell.CellValue != null)
                    {
                        int columnIndex = GetColumnIndex(cell.CellReference.ToString());
                        int rowIndex = GetRowIndex(cell.CellReference.ToString(), containHeaderRow);
                        string text = cell.CellValue.InnerXml;
                        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                        {
                            text = stringTablePart.SharedStringTable.ChildElements[int.Parse(text, CultureInfo.InvariantCulture)].InnerText;
                        }

                        excelRawValue[columnIndex, rowIndex] = text;
                    }
                });
            });
            return excelRawValue;
        }

        private static (int ColumnCount, int RowCount) CalculateExcelTableSize(Worksheet worksheet, bool containHeaderRow)
        {
            string[] array = worksheet.SheetDimension.Reference.InnerText.Split(':');
            int item = 1 + GetColumnIndex(array[1]);
            int item2 = 1 + GetRowIndex(array[1], containHeaderRow);
            return (item, item2);
        }

        private static int GetRowIndex(string cellReference, bool containHeaderRow)
        {
            int rowIndex = OpenXmlConvert.GetRowIndex(cellReference);
            if (!containHeaderRow)
            {
                return rowIndex - 1;
            }

            return rowIndex - 2;
        }

        private static int GetColumnIndex(string cellReference)
        {
            string text = Regex.Replace(cellReference, "[^A-Z_]", "");
            int num = 0;
            int num2 = 1;
            for (int num3 = text.Length - 1; num3 >= 0; num3--)
            {
                num += (text[num3] - 65 + 1) * num2;
                num2 *= 26;
            }

            return num - 1;
        }
    }
}
