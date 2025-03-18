namespace TN.Excel.Attributes
{
    //
    // Summary:
    //     Column required attribute
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExcelColumnRequiredAttribute : Attribute
    {
        //
        // Summary:
        //     Get Value
        public bool Value { get; }

        //
        // Summary:
        //     This is a positional argument
        //
        // Parameters:
        //   value:
        //     bool
        public ExcelColumnRequiredAttribute(bool value)
        {
            Value = value;
        }
    }
}
