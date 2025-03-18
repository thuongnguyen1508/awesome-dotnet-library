using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

namespace TN.Excel.Exporting
{
    //
    // Summary:
    //     OpenXML Init Helper
    public static class OpenXmlSetup
    {
        private const string FontName = "Calibri";

        private const string CellStyleName = "Normal";

        private const string DefaultDateTimeFormat = "dd/MM/yyyy HH:mm:ss";

        //
        // Summary:
        //     Number format codes less than 164 are "built-in". To create custom number format
        //     start from 164.
        private static int _customNumberFormatId = 164;

        //
        // Summary:
        //     Generate Excel Style Sheet
        //
        // Returns:
        //     The specific default style sheet
        private static Stylesheet GetStylesheet(string customDateTimeFormat)
        {
            if (customDateTimeFormat == null)
            {
                customDateTimeFormat = "dd/MM/yyyy HH:mm:ss";
            }

            Stylesheet stylesheet = new Stylesheet();
            Fonts fonts = new Fonts();
            Font newChild = new Font
            {
                FontName = new FontName
                {
                    Val = "Calibri"
                },
                FontSize = new FontSize
                {
                    Val = 11.0
                },
                FontFamilyNumbering = new FontFamilyNumbering
                {
                    Val = 2
                }
            };
            fonts.AppendChild(newChild);
            fonts.Count = (uint)fonts.ChildElements.Count;
            Fills fills = new Fills();
            Fill newChild2 = new Fill
            {
                PatternFill = new PatternFill
                {
                    PatternType = PatternValues.None
                }
            };
            fills.AppendChild(newChild2);
            Fill newChild3 = new Fill
            {
                PatternFill = new PatternFill
                {
                    PatternType = PatternValues.Gray125
                }
            };
            fills.AppendChild(newChild3);
            fills.Count = (uint)fills.ChildElements.Count;
            Borders borders = new Borders();
            Border newChild4 = new Border
            {
                LeftBorder = new LeftBorder(),
                RightBorder = new RightBorder(),
                TopBorder = new TopBorder(),
                BottomBorder = new BottomBorder(),
                DiagonalBorder = new DiagonalBorder()
            };
            borders.AppendChild(newChild4);
            borders.Count = (uint)borders.ChildElements.Count;
            CellStyleFormats cellStyleFormats = new CellStyleFormats();
            CellFormat newChild5 = new CellFormat
            {
                NumberFormatId = 0u,
                FontId = 0u,
                FillId = 0u,
                BorderId = 0u
            };
            cellStyleFormats.AppendChild(newChild5);
            cellStyleFormats.Count = (uint)cellStyleFormats.ChildElements.Count;
            CellFormats cellFormats = new CellFormats();
            CellFormat newChild6 = new CellFormat
            {
                BorderId = 0u,
                FillId = 0u,
                FontId = 0u,
                NumberFormatId = 0u,
                FormatId = 0u,
                ApplyNumberFormat = true
            };
            cellFormats.AppendChild(newChild6);
            NumberingFormats numberingFormats = new NumberingFormats();
            NumberingFormat numberingFormat = new NumberingFormat
            {
                NumberFormatId = UInt32Value.FromUInt32((uint)_customNumberFormatId++),
                FormatCode = StringValue.FromString(customDateTimeFormat)
            };
            numberingFormats.AppendChild(numberingFormat);
            CellFormat newChild7 = new CellFormat
            {
                BorderId = 0u,
                FillId = 0u,
                FontId = 0u,
                NumberFormatId = numberingFormat.NumberFormatId,
                FormatId = 0u,
                ApplyNumberFormat = true
            };
            cellFormats.AppendChild(newChild7);
            numberingFormats.Count = UInt32Value.FromUInt32((uint)numberingFormats.ChildElements.Count);
            cellFormats.Count = (uint)cellFormats.ChildElements.Count;
            CellStyles cellStyles = new CellStyles();
            CellStyle newChild8 = new CellStyle
            {
                Name = "Normal",
                FormatId = 0u,
                BuiltinId = 0u
            };
            cellStyles.AppendChild(newChild8);
            cellStyles.Count = (uint)cellStyles.ChildElements.Count;
            stylesheet.AppendChild(numberingFormats);
            stylesheet.AppendChild(fonts);
            stylesheet.AppendChild(fills);
            stylesheet.AppendChild(borders);
            stylesheet.AppendChild(cellStyleFormats);
            stylesheet.AppendChild(cellFormats);
            stylesheet.AppendChild(cellStyles);
            return stylesheet;
        }

        //
        // Summary:
        //     Quick init spread sheet document from memory stream
        //
        // Parameters:
        //   sheetName:
        //     Sheet name
        //
        //   customDateTimeFormat:
        //     Custom date time format
        //
        //   sheetDimension:
        //     Sheet dimension
        //
        //   sheetData:
        //     Sheet data
        //
        //   document:
        //     Spread sheet document
        internal static void InitOpenXml(SpreadsheetDocument document, string sheetName, SheetDimension sheetDimension, string customDateTimeFormat, out SheetData sheetData)
        {
            WorkbookPart workbookPart = document.AddWorkbookPart();
            WorkbookStylesPart workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            workbookStylesPart.Stylesheet = GetStylesheet(customDateTimeFormat);
            workbookStylesPart.Stylesheet.Save();
            workbookPart.Workbook = new Workbook();
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(sheetData)
            {
                SheetDimension = sheetDimension
            };
            workbookPart.Workbook.AppendChild(new Sheets()).AppendChild(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1u,
                Name = sheetName
            });
        }
    }
}
