namespace KKL.WordStudio.Infrastructure.Tests;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using KKL.WordStudio.Infrastructure.Excel;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class OpenXmlExcelWorkbookReaderTests
{
    [Fact]
    public async Task OpenWorkbookAsync_ListsAllSheetNames()
    {
        var filePath = CreateSampleWorkbook();
        var reader = new OpenXmlExcelWorkbookReader(NullLogger<OpenXmlExcelWorkbookReader>.Instance);

        var result = await reader.OpenWorkbookAsync(filePath);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Worksheets);
        Assert.Equal("Sales", result.Value.Worksheets[0].Name);

        File.Delete(filePath);
    }

    [Fact]
    public async Task GetSheetPreviewAsync_ReadsHeaderAndDataRows()
    {
        var filePath = CreateSampleWorkbook();
        var reader = new OpenXmlExcelWorkbookReader(NullLogger<OpenXmlExcelWorkbookReader>.Instance);

        var result = await reader.GetSheetPreviewAsync(filePath, "Sales");

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Rows.Count); // header + 2 data rows
        Assert.Equal("CustomerName", result.Value.Rows[0][0]);
        Assert.Equal("Alice", result.Value.Rows[1][0]);

        File.Delete(filePath);
    }

    [Fact]
    public async Task DetectDataRangeAsync_FindsLastContiguousDataRow()
    {
        var filePath = CreateSampleWorkbook();
        var reader = new OpenXmlExcelWorkbookReader(NullLogger<OpenXmlExcelWorkbookReader>.Instance);

        var result = await reader.DetectDataRangeAsync(filePath, "Sales", dataStartRow: 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.DataEndRow); // rows 2 and 3 are data; row 1 is the header, excluded by dataStartRow
        Assert.True(result.Value.WasAutoDetected);

        File.Delete(filePath);
    }

    /// <summary>Builds a minimal real .xlsx (one "Sales" sheet, header row + two data rows) using the OpenXML SDK directly — this is what proves the reader works against actual OpenXML output, not a hand-rolled fake.</summary>
    private static string CreateSampleWorkbook()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".xlsx");

        using (var document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Sales"
            });

            sheetData.Append(BuildRow(1, "CustomerName", "Amount"));
            sheetData.Append(BuildRow(2, "Alice", "100"));
            sheetData.Append(BuildRow(3, "Bob", "200"));

            workbookPart.Workbook.Save();
        }

        return filePath;
    }

    private static Row BuildRow(uint rowIndex, params string[] values)
    {
        var row = new Row { RowIndex = rowIndex };
        foreach (var value in values)
            row.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue(value) });
        return row;
    }
}
