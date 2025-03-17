namespace TN.Excel.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExcelExportInfomationAttribute : Attribute
    {
        //
        // Summary:
        //     Get custom header name
        public string CellAddress { get; }

        //
        // Summary:
        //     Format of value want to To String
        public string Format { get; }

        //
        // Summary:
        //     Constructor
        //
        // Parameters:
        //   cellAddress:
        //     Cell Address
        //
        //   format:
        public ExcelExportInfomationAttribute(string cellAddress, string format = null)
        {
            if (string.IsNullOrWhiteSpace(cellAddress))
            {
                throw new ArgumentNullException("cellAddress");
            }

            CellAddress = cellAddress;
            Format = format;
        }
    }
}
