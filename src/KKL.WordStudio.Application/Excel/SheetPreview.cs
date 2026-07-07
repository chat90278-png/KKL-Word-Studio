namespace KKL.WordStudio.Application.Excel;

/// <summary>
/// A bounded preview of a worksheet's raw cell contents, used by the Excel
/// Workspace UI to render a grid and let the user pick a start row before
/// committing to a full data-range/column-mapping configuration. Rows are
/// raw text — no type inference, no header/data distinction yet, since
/// that's exactly what the user is about to decide.
/// </summary>
public sealed class SheetPreview
{
    public required string WorksheetName { get; init; }

    /// <summary>1-based row numbers matching Rows, so the UI can label rows correctly even if PreviewRowLimit truncated the sheet.</summary>
    public required IReadOnlyList<int> RowNumbers { get; init; }

    /// <summary>Raw cell text per row, outer index aligned with RowNumbers, inner index is 0-based column.</summary>
    public required IReadOnlyList<IReadOnlyList<string>> Rows { get; init; }

    public required int ColumnCount { get; init; }

    /// <summary>True if the sheet has more rows than were returned in this preview.</summary>
    public bool IsTruncated { get; init; }
}
