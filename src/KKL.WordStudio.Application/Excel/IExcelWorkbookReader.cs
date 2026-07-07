namespace KKL.WordStudio.Application.Excel;

using KKL.WordStudio.Domain.DataSources;
using KKL.WordStudio.Shared.Results;

/// <summary>
/// Application-facing contract for browsing an Excel file at design time:
/// listing its sheets and previewing raw cell content so the user can pick
/// a start row and let the system detect where the data ends. Distinct
/// from IDataProvider (which fetches actual typed rows for report
/// execution once a DataSource is fully configured) — this interface is
/// purely about the import/configuration workflow in the Excel Workspace.
/// Implemented in Infrastructure using the OpenXML SDK.
/// </summary>
public interface IExcelWorkbookReader
{
    /// <summary>Opens the workbook and returns its structural description (sheet names) without reading cell data.</summary>
    Task<Result<Workbook>> OpenWorkbookAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>Reads up to <paramref name="maxPreviewRows"/> raw rows from a worksheet, for display while the user configures the data range.</summary>
    Task<Result<SheetPreview>> GetSheetPreviewAsync(
        string filePath, string worksheetName, int maxPreviewRows = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans the worksheet starting at <paramref name="dataStartRow"/> and returns a DataRange with
    /// DataEndRow set to the last row of a contiguous non-blank block (WasAutoDetected = true).
    /// The caller may subsequently overwrite DataEndRow manually, at which point it should also
    /// set WasAutoDetected = false to record that the value is no longer the system's guess.
    /// </summary>
    Task<Result<DataRange>> DetectDataRangeAsync(
        string filePath, string worksheetName, int dataStartRow, CancellationToken cancellationToken = default);
}
