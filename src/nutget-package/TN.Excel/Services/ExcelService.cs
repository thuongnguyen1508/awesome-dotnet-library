using TN.Excel.Importing;
using TN.Excel.Services.Abstractions;

namespace TN.Excel.Services
{
    public class ExcelService : IExcelService
    {
        public Stream Export<TData>(IEnumerable<TData> inputData, string reportName)
        {
            var excelData = inputData.ToList();

            return excelData.GetExcel(reportName);//default format is dd/MM/yyyy HH:mm:ss
        }

        public List<TData> ReadExcelFromStream<TData>(Stream stream, string sheetName, bool containHeaderRow) where TData : new()
        {
            var listData = OpenXmlImporter.ReadExcelFromStream<TData>(stream, sheetName, true);
            return listData;
        }
    }
}
