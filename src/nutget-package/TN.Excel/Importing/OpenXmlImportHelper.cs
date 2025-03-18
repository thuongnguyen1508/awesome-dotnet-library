using System.Globalization;
using System.Reflection;
using TN.Excel.Attributes;

namespace TN.Excel.Importing
{
    //
    // Summary:
    //     Parse helper
    public static class OpenXmlImportHelper
    {
        private const string ExcelNullValue = "#N/A";

        //
        // Summary:
        //     Default type converter
        private static readonly Dictionary<Type, Func<string, object>> DefaultTypeConverter = new Dictionary<Type, Func<string, object>>
    {
        {
            typeof(DateTime),
            DateTimeFromExcel
        },
        {
            typeof(Uri),
            UriFromExcel
        },
        {
            typeof(bool),
            BooleanFromExcel
        },
        {
            typeof(TimeSpan),
            TimeSpanFromExcel
        }
    };

        //
        // Summary:
        //     Parse raw excel value
        //
        // Parameters:
        //   rawData:
        //     Two dimension of raw excel data
        //
        //   typeConverter:
        //     Dictionary
        //
        // Type parameters:
        //   TExcelData:
        //     Dynamic type of object input
        //
        // Exceptions:
        //   T:System.NotSupportedException:
        //     Excel property is required but value is null
        internal static List<TExcelData> Parse<TExcelData>(string[,] rawData, Dictionary<Type, Func<string, object>> typeConverter = null) where TExcelData : new()
        {
            if (rawData == null)
            {
                throw new ArgumentNullException("rawData");
            }

            if (typeConverter == null)
            {
                typeConverter = DefaultTypeConverter;
            }

            int length = rawData.GetLength(1);
            List<PropertyInfo> list = typeof(TExcelData).GetProperties().ToList();
            List<TExcelData> excelDataList = new List<TExcelData>(length);
            int rowIndex;
            for (rowIndex = 0; rowIndex < length; rowIndex++)
            {
                excelDataList.Add(new TExcelData());
                list.ForEach(delegate (PropertyInfo property)
                {
                    ExcelColumnIndexAttribute customAttribute = property.GetCustomAttribute<ExcelColumnIndexAttribute>();
                    if (customAttribute != null)
                    {
                        ExcelColumnRequiredAttribute customAttribute2 = property.GetCustomAttribute<ExcelColumnRequiredAttribute>();
                        object obj = Parse(property.PropertyType, rawData[customAttribute.Index, rowIndex], typeConverter);
                        if (customAttribute2 != null && customAttribute2.Value && obj == null)
                        {
                            throw new NotSupportedException($"[{typeof(TExcelData).Name}] [Column name: {property.Name}] [Row:{rowIndex}][Column:{customAttribute.Index}] ");
                        }

                        property.SetValue(excelDataList[rowIndex], obj);
                    }
                });
            }

            return excelDataList;
        }

        //
        // Summary:
        //     Parse raw value to exactly C# property value based on type converter
        //
        // Parameters:
        //   excelCellValue:
        //     Value need to be parse
        //
        //   typeNeedToParse:
        //     Property type
        //
        //   typeConverter:
        //     Type Converter
        private static object Parse(Type typeNeedToParse, string excelCellValue, Dictionary<Type, Func<string, object>> typeConverter = null)
        {
            if (typeConverter == null)
            {
                typeConverter = DefaultTypeConverter;
            }

            if (string.IsNullOrWhiteSpace(excelCellValue) || excelCellValue.Equals("#N/A", StringComparison.Ordinal))
            {
                return null;
            }

            if (typeConverter.ContainsKey(typeNeedToParse))
            {
                return typeConverter[typeNeedToParse](excelCellValue);
            }

            try
            {
                return Convert.ChangeType(excelCellValue, typeNeedToParse, CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is OverflowException)
            {
                return null;
            }
        }

        private static object TimeSpanFromExcel(string rawValue)
        {
            if (!double.TryParse(rawValue, out var result))
            {
                return null;
            }

            return DateTime.FromOADate(result).TimeOfDay;
        }

        private static object DateTimeFromExcel(string rawValue)
        {
            if (!double.TryParse(rawValue, out var result))
            {
                return null;
            }

            return DateTime.FromOADate(result);
        }

        private static object UriFromExcel(string rawValue)
        {
            Uri.TryCreate(rawValue, UriKind.Absolute, out Uri result);
            return result;
        }

        private static object BooleanFromExcel(string rawValue)
        {
            switch (rawValue.ToUpperInvariant())
            {
                case "YES":
                case "TRUE":
                case "1":
                    return true;
                default:
                    return false;
            }
        }
    }
}
