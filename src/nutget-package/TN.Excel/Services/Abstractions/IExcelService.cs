namespace TN.Excel.Services.Abstractions
{
    public interface IExcelService
    {
        List<TData> ReadExcelFromStream<TData>(Stream stream, string sheetName, bool containHeaderRow) where TData: new();
        Stream Export<TData>(IEnumerable<TData> inputData, string reportName);
    }
}
