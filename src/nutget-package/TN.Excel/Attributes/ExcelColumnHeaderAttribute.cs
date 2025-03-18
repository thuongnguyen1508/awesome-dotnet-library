namespace TN.Excel.Attributes
{
    //
    // Summary:
    //     Custom header name for export
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExcelColumnHeaderAttribute : Attribute
    {
        //
        // Summary:
        //     Get custom header name
        public string Name { get; }

        //
        // Summary:
        //     Constructor
        //
        // Parameters:
        //   columnHeaderName:
        //     Header name
        public ExcelColumnHeaderAttribute(string columnHeaderName)
        {
            Name = columnHeaderName;
        }
    }
}
