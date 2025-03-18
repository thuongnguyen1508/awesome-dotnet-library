namespace TN.Excel.Attributes
{
    //
    // Summary:
    //     Excel column index attribute
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExcelColumnIndexAttribute : Attribute
    {
        //
        // Summary:
        //     Get index
        public byte Index { get; }

        //
        // Summary:
        //     This is a positional argument
        //
        // Parameters:
        //   index:
        //     byte
        public ExcelColumnIndexAttribute(byte index)
        {
            Index = index;
        }
    }
}
