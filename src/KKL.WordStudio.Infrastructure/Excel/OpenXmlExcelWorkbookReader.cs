namespace KKL.WordStudio.Infrastructure.Excel;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using KKL.WordStudio.Application.Excel;
using KKL.WordStudio.Domain.DataSources;
using KKL.WordStudio.Shared.Results;
using KKL.WordStudio.Shared.Spreadsheet;
using Microsoft.Extensions.Logging;

/// <summary>
/// Reads .xlsx files using the OpenXML SDK — the same package already
/// planned for Word export, so no extra dependency was introduced to
/// support Excel import. Read-only: this class never writes to the
/// workbook, matching the Sprint 2 scope ("henüz editing gerekmiyor").
/// </summary>
public sealed class OpenXmlExcelWorkbookReader : IExcelWorkbookReader
{
    private readonly ILogger<OpenXmlExcelWorkbookReader> _logger;

    public OpenXmlExcelWorkbookReader(ILogger<OpenXmlExcelWorkbookReader> logger) => _logger = logger;

    public Task<Result<Workbook>> OpenWorkbookAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
                return Task.FromResult(Result.Failure<Workbook>($"Excel file not found: {filePath}"));

            using var document = SpreadsheetDocument.Open(filePath, false);
            var workbookPart = document.WorkbookPart
                ?? throw new InvalidDataException("The workbook has no WorkbookPart.");

            var workbook = new Workbook { FileName = Path.GetFileName(filePath) };

            var sheets = workbookPart.Workbook.Sheets?.Elements<Sheet>() ?? Enumerable.Empty<Sheet>();
            foreach (var sheet in sheets)
            {
                if (sheet.Name is null) continue;
                workbook.Worksheets.Add(new Worksheet { Name = sheet.Name.Value! });
            }

            return Task.FromResult(Result.Success(workbook));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open workbook {FilePath}", filePath);
            return Task.FromResult(Result.Failure<Workbook>($"Could not open Excel file: {ex.Message}"));
        }
    }

    public Task<Result<SheetPreview>> GetSheetPreviewAsync(
        string filePath, string worksheetName, int maxPreviewRows = 100, CancellationToken cancellationToken = default)
    {
        try
        {
            using var document = SpreadsheetDocument.Open(filePath, false);
            var (worksheetPart, sharedStrings) = OpenWorksheetPart(document, worksheetName);

            var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
            var rowNumbers = new List<int>();
            var rows = new List<IReadOnlyList<string>>();
            var maxColumnCount = 0;
            var truncated = false;
            var readCount = 0;

            foreach (var row in sheetData.Elements<Row>())
            {
                if (readCount >= maxPreviewRows)
                {
                    truncated = true;
                    break;
                }

                var rowIndex = (int)(row.RowIndex?.Value ?? (uint)(readCount + 1));
                var cellsInRow = ReadRowCells(row, sharedStrings);

                rowNumbers.Add(rowIndex);
                rows.Add(cellsInRow);
                maxColumnCount = Math.Max(maxColumnCount, cellsInRow.Count);
                readCount++;
            }

            var preview = new SheetPreview
            {
                WorksheetName = worksheetName,
                RowNumbers = rowNumbers,
                Rows = rows,
                ColumnCount = maxColumnCount,
                IsTruncated = truncated
            };

            return Task.FromResult(Result.Success(preview));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to preview sheet {Sheet} in {FilePath}", worksheetName, filePath);
            return Task.FromResult(Result.Failure<SheetPreview>($"Could not preview sheet: {ex.Message}"));
        }
    }

    public Task<Result<DataRange>> DetectDataRangeAsync(
        string filePath, string worksheetName, int dataStartRow, CancellationToken cancellationToken = default)
    {
        try
        {
            using var document = SpreadsheetDocument.Open(filePath, false);
            var (worksheetPart, sharedStrings) = OpenWorksheetPart(document, worksheetName);
            var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

            int? lastNonBlankRow = null;
            var minColumn = int.MaxValue;
            var maxColumn = 0;
            var sawBlankAfterData = false;

            foreach (var row in sheetData.Elements<Row>())
            {
                var rowIndex = (int)(row.RowIndex?.Value ?? 0);
                if (rowIndex < dataStartRow) continue;
                if (sawBlankAfterData) break;

                var cells = ReadRowCells(row, sharedStrings);
                var hasAnyValue = cells.Any(c => !string.IsNullOrWhiteSpace(c));

                if (!hasAnyValue)
                {
                    if (lastNonBlankRow.HasValue) sawBlankAfterData = true;
                    continue;
                }

                lastNonBlankRow = rowIndex;

                foreach (var cell in row.Elements<Cell>())
                {
                    if (cell.CellReference?.Value is null) continue;
                    var (letters, _) = ColumnLetterConverter.SplitCellReference(cell.CellReference.Value);
                    var columnIndex = ColumnLetterConverter.ToIndex(letters);
                    minColumn = Math.Min(minColumn, columnIndex);
                    maxColumn = Math.Max(maxColumn, columnIndex);
                }
            }

            var range = new DataRange
            {
                DataStartRow = dataStartRow,
                DataEndRow = lastNonBlankRow,
                StartColumn = minColumn == int.MaxValue ? null : minColumn,
                EndColumn = maxColumn == 0 ? null : maxColumn,
                WasAutoDetected = true
            };

            return Task.FromResult(Result.Success(range));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect data range in {Sheet} of {FilePath}", worksheetName, filePath);
            return Task.FromResult(Result.Failure<DataRange>($"Could not detect data range: {ex.Message}"));
        }
    }

    private static (WorksheetPart Part, SharedStringTable? SharedStrings) OpenWorksheetPart(SpreadsheetDocument document, string worksheetName)
    {
        var workbookPart = document.WorkbookPart
            ?? throw new InvalidDataException("The workbook has no WorkbookPart.");

        var sheet = workbookPart.Workbook.Sheets?.Elements<Sheet>()
            .FirstOrDefault(s => s.Name == worksheetName)
            ?? throw new InvalidDataException($"Worksheet '{worksheetName}' was not found.");

        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!.Value!);
        var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;

        return (worksheetPart, sharedStrings);
    }

    private static IReadOnlyList<string> ReadRowCells(Row row, SharedStringTable? sharedStrings)
    {
        var cells = new List<string>();
        foreach (var cell in row.Elements<Cell>())
            cells.Add(GetCellText(cell, sharedStrings));
        return cells;
    }

    private static string GetCellText(Cell cell, SharedStringTable? sharedStrings)
    {
        var rawValue = cell.CellValue?.Text ?? string.Empty;

        if (cell.DataType?.Value == CellValues.SharedString)
        {
            if (sharedStrings is null || !int.TryParse(rawValue, out var sharedStringIndex))
                return string.Empty;

            var sharedItem = sharedStrings.ElementAtOrDefault(sharedStringIndex);
            return sharedItem?.InnerText ?? string.Empty;
        }

        if (cell.DataType?.Value == CellValues.Boolean)
            return rawValue == "1" ? "TRUE" : "FALSE";

        return rawValue;
    }
}
