using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using TN.Excel.Attributes;
using TN.Excel.Exporting;

namespace TN.Excel
{
    public static class OpenXmlListExtensions
    {
        private const string SheetNameInvalid = "A worksheet name cannot exceed 31 characters and cannot contain any of the following characters: \\ , / , * , ? , : , [ , ] ";

        private const string SheetNameRegex = "^[^\\/\\\\\\?\\*\\[\\]]{1,31}$";

        private const string SheetDimension = "A1:{0}{1}";

        private const int DefaultBufferSize = 4096;

        //
        // Summary:
        //     Get excel template after fill data as bytes array from instance
        //
        // Parameters:
        //   exportData:
        //     List export data
        //
        //   templateFilePath:
        //     Excel template file path
        //
        //   sheetName:
        //     Excel Sheet name
        //
        // Type parameters:
        //   TExportData:
        //     Type of data export
        //
        // Returns:
        //     Excel Stream
        //
        // Exceptions:
        //   T:System.NullReferenceException:
        //     Exception thrown when export data is null
        //
        //   T:System.ArgumentNullException:
        //     Exception thrown when file path is null
        //
        //   T:System.ArgumentException:
        //     Exception thrown when sheet name is invalid
        //
        //   T:System.ArgumentOutOfRangeException:
        //     Exception throw when invalid cell address or cell address out of dimension
        //
        //   T:System.IO.FileNotFoundException:
        //     Exception thrown when template file not found
        public static async Task<Stream> GetExcelUsingTemplateAsync<TExportData>(this TExportData exportData, string templateFilePath, string sheetName)
        {
            if (exportData == null)
            {
                throw new NullReferenceException();
            }

            Stream result;
            await using (FileStream fileStream = new FileStream(templateFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                Stream stream = new MemoryStream(0);
                await fileStream.CopyToAsync(stream).ConfigureAwait(continueOnCapturedContext: false);
                SpreadsheetDocument document = SpreadsheetDocument.Open(stream, isEditable: true);
                try
                {
                    StringValue sheetIdValue = (from sheet in document.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>()
                                                where sheet.Name == sheetName
                                                select sheet.Id.Value).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(sheetIdValue))
                    {
                        throw new ArgumentException(null, "sheetName");
                    }

                    List<PropertyInfo> list = (from prop in typeof(TExportData).GetProperties()
                                               where prop.GetCustomAttribute<ExcelExportInfomationAttribute>() != null
                                               select prop).ToList();
                    if (!list.Any())
                    {
                        result = Stream.Null;
                    }
                    else
                    {
                        try
                        {
                            list.ForEach(delegate (PropertyInfo propertyInfo)
                            {
                                ExcelExportInfomationAttribute excelExportInfomationAttribute = propertyInfo.GetCustomAttributes<ExcelExportInfomationAttribute>().FirstOrDefault();
                                if (excelExportInfomationAttribute != null)
                                {
                                    object value = propertyInfo.GetValue(exportData);
                                    string textValue = ((!string.IsNullOrWhiteSpace(excelExportInfomationAttribute.Format)) ? ((value is object[] args) ? string.Format(CultureInfo.CurrentCulture, excelExportInfomationAttribute.Format, args) : string.Format(CultureInfo.CurrentCulture, excelExportInfomationAttribute.Format, value as IFormattable)) : value?.ToString()) ?? string.Empty;
                                    OpenXmlAction.AppendValueToCell(document.WorkbookPart, sheetIdValue, excelExportInfomationAttribute.CellAddress, textValue);
                                }
                            });
                            ((WorksheetPart)document.WorkbookPart.GetPartById(sheetIdValue)).Worksheet.Save();
                            document.WorkbookPart.Workbook.Save();
                        }
                        catch (Exception ex) when (ex is OpenXmlPackageException || ex is InvalidOperationException)
                        {
                            result = Stream.Null;
                            goto end_IL_0115;
                        }

                        document.Save();
                        document.Dispose();
                        stream.Position = 0L;
                        result = stream;
                    }

                end_IL_0115:;
                }
                finally
                {
                    if (document != null)
                    {
                        ((IDisposable)document).Dispose();
                    }
                }
            }

            return result;
        }

        //
        // Summary:
        //     Get excel file as bytes array
        //
        // Parameters:
        //   exportData:
        //     List of TExcelData
        //
        //   sheetName:
        //     Sheet saving name
        //
        //   customDateTimeFormat:
        //     Format of datetime
        //
        // Type parameters:
        //   TExcelData:
        //     Generic type of list data input
        //
        // Returns:
        //     Stream excel
        //
        // Exceptions:
        //   T:System.NullReferenceException:
        //     Exception thrown when sheet name is null
        //
        //   T:System.ArgumentNullException:
        //     Exception thrown when export data is null
        //
        //   T:System.ArgumentException:
        //     Exception thrown when sheet name invalid
        public static Stream GetExcel<TExcelData>(this List<TExcelData> exportData, string sheetName, string customDateTimeFormat = null)
        {
            if (exportData == null)
            {
                throw new NullReferenceException();
            }

            if (string.IsNullOrEmpty(sheetName))
            {
                throw new ArgumentNullException("sheetName");
            }

            if (!new Regex("^[^\\/\\\\\\?\\*\\[\\]]{1,31}$").IsMatch(sheetName))
            {
                throw new ArgumentException("A worksheet name cannot exceed 31 characters and cannot contain any of the following characters: \\ , / , * , ? , : , [ , ] ", "sheetName");
            }

            List<PropertyInfo> list = (from prop in typeof(TExcelData).GetProperties()
                                       where prop.GetCustomAttribute<ExcelColumnHeaderAttribute>() != null
                                       select prop).ToList();
            if (!list.Any())
            {
                return Stream.Null;
            }

            SheetDimension sheetDimension = new SheetDimension
            {
                Reference = string.Format(CultureInfo.CurrentCulture, "A1:{0}{1}", OpenXmlConvert.ToExcelColumnName(list.Count - 1), exportData.Count + 1)
            };
            Stream stream = new MemoryStream();
            using SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
            OpenXmlSetup.InitOpenXml(spreadsheetDocument, sheetName, sheetDimension, customDateTimeFormat, out var sheetData);
            OpenXmlAction.AppendHeaderNames(sheetData, list);
            sheetData.AppendRange(exportData, list);
            try
            {
                spreadsheetDocument.WorkbookPart.Workbook.Save();
            }
            catch (InvalidOperationException)
            {
                return Stream.Null;
            }

            spreadsheetDocument.Save();
            spreadsheetDocument.Dispose();
            stream.Position = 0L;
            return stream;
        }
    }
}
