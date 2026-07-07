namespace KKL.WordStudio.Infrastructure.Excel;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using KKL.WordStudio.Application.Abstractions;
using KKL.WordStudio.Domain.DataBinding;
using KKL.WordStudio.Domain.DataSources;
using KKL.WordStudio.Shared.Results;
using KKL.WordStudio.Shared.Spreadsheet;
using Microsoft.Extensions.Logging;

/// <summary>
/// The first real (non-in-memory) IDataProvider: reads actual row data for
/// an ExcelDataSource from its configured Worksheet/DataRange, using
/// ColumnMappings to translate raw spreadsheet columns into the logical
/// field names report elements bind to. This is what makes bound-table
/// export/preview show real data instead of a placeholder.
///
/// Values are returned as strings (no type coercion from DataField.DataType
/// yet) — sufficient for Sprint 4's text-based Word/Preview output; numeric
/// formatting is deferred to whenever formatted export is actually needed.
/// </summary>
public sealed class ExcelDataProvider : IDataProvider
{
    private readonly ILogger<ExcelDataProvider> _logger;

    public ExcelDataProvider(ILogger<ExcelDataProvider> logger) => _logger = logger;

    public string ProviderKey => "excel";

    public Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> GetRowsAsync(
        IDataSourceDefinition definition, CancellationToken cancellationToken = default)
    {
        if (definition is not ExcelDataSource excelDataSource)
            return Task.FromResult(Result.Failure<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(
                $"ExcelDataProvider cannot read a data source of type '{definition.GetType().Name}'."));

        var sourcePath = excelDataSource.Workbook.SourcePath;
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            return Task.FromResult(Result.Failure<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(
                $"Excel source file not found: '{sourcePath}'. Re-link the data source in the Excel Workspace."));

        var worksheetName = excelDataSource.ActiveWorksheetName;
        var worksheet = excelDataSource.Workbook.Worksheets.FirstOrDefault(w => w.Name == worksheetName);
        var range = worksheet?.SelectedRange;

        if (range is null)
            return Task.FromResult(Result.Failure<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(
                $"Data source '{excelDataSource.Name}' has no configured data range."));

        try
        {
            using var document = SpreadsheetDocument.Open(sourcePath, false);
            var workbookPart = document.WorkbookPart ?? throw new InvalidDataException("The workbook has no WorkbookPart.");
            var sheet = workbookPart.Workbook.Sheets?.Elements<Sheet>().FirstOrDefault(s => s.Name == worksheetName)
                ?? throw new InvalidDataException($"Worksheet '{worksheetName}' was not found.");
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!.Value!);
            var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;
            var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

            var rows = new List<IReadOnlyDictionary<string, object?>>();

            foreach (var row in sheetData.Elements<Row>())
            {
                var rowIndex = (int)(row.RowIndex?.Value ?? 0);
                if (rowIndex < range.DataStartRow) continue;
                if (range.DataEndRow.HasValue && rowIndex > range.DataEndRow.Value) break;

                var cellValuesByColumn = new Dictionary<int, string>();
                foreach (var cell in row.Elements<Cell>())
                {
                    if (cell.CellReference?.Value is null) continue;
                    var (letters, _) = ColumnLetterConverter.SplitCellReference(cell.CellReference.Value);
                    cellValuesByColumn[ColumnLetterConverter.ToIndex(letters)] = GetCellText(cell, sharedStrings);
                }

                var record = new Dictionary<string, object?>();
                foreach (var mapping in excelDataSource.ColumnMappings)
                {
                    var columnIndex = ColumnLetterConverter.ToIndex(mapping.SourceColumn);
                    record[mapping.TargetField.Name] = cellValuesByColumn.TryGetValue(columnIndex, out var value) ? value : null;
                }

                rows.Add(record);
            }

            return Task.FromResult(Result.Success<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(rows));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read rows for data source {DataSource} from {Path}", excelDataSource.Name, sourcePath);
            return Task.FromResult(Result.Failure<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(
                $"Could not read Excel data: {ex.Message}"));
        }
    }

    private static string GetCellText(Cell cell, SharedStringTable? sharedStrings)
    {
        var rawValue = cell.CellValue?.Text ?? string.Empty;

        if (cell.DataType?.Value == CellValues.SharedString)
        {
            if (sharedStrings is null || !int.TryParse(rawValue, out var index)) return string.Empty;
            return sharedStrings.ElementAtOrDefault(index)?.InnerText ?? string.Empty;
        }

        if (cell.DataType?.Value == CellValues.Boolean)
            return rawValue == "1" ? "TRUE" : "FALSE";

        return rawValue;
    }
}
