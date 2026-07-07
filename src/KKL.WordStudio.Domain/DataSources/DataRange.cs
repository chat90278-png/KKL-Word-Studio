namespace KKL.WordStudio.Domain.DataSources;

using System.Text.Json.Serialization;
using KKL.WordStudio.Shared.Spreadsheet;

/// <summary>
/// The rectangular region within a Worksheet that actually holds tabular
/// data (as opposed to titles, blank rows, notes, etc.).
///
/// As of Sprint 2, this is structured (row/column indices + provenance
/// flags) rather than a single opaque A1-string. A string alone cannot
/// represent "user picked the start row, the system auto-detected the end
/// row, then the user manually corrected it" without re-parsing on every
/// change — the structured fields make that provenance explicit, and
/// <see cref="RangeReference"/> is now a computed display value rather
/// than a second, independently-stored source of truth.
/// </summary>
public sealed class DataRange
{
    /// <summary>1-based row where the first record of actual data begins (user-selectable).</summary>
    public int DataStartRow { get; set; } = 1;

    /// <summary>1-based row where data ends. Null until detected or set.</summary>
    public int? DataEndRow { get; set; }

    /// <summary>1-based row containing column headers, if any. Null means "no header row".</summary>
    public int? HeaderRowIndex { get; set; }

    /// <summary>1-based start column. Null means "use whatever the sheet's used range starts at".</summary>
    public int? StartColumn { get; set; }

    /// <summary>1-based end column. Null means "use whatever the sheet's used range ends at".</summary>
    public int? EndColumn { get; set; }

    /// <summary>True if DataEndRow was computed by auto-detection; false if the user manually overrode it.</summary>
    public bool WasAutoDetected { get; set; }

    public bool HasHeaderRow => HeaderRowIndex.HasValue;

    /// <summary>Human-readable A1-style reference for display only — always derived, never stored, so it can never drift out of sync with the structured fields above.</summary>
    [JsonIgnore]
    public string RangeReference
    {
        get
        {
            if (StartColumn is null || EndColumn is null)
                return DataEndRow.HasValue ? $"Row {DataStartRow}:{DataEndRow}" : $"Row {DataStartRow}:?";

            var startCell = $"{ColumnLetterConverter.ToLetters(StartColumn.Value)}{DataStartRow}";
            var endCell = DataEndRow.HasValue
                ? $"{ColumnLetterConverter.ToLetters(EndColumn.Value)}{DataEndRow.Value}"
                : $"{ColumnLetterConverter.ToLetters(EndColumn.Value)}?";
            return $"{startCell}:{endCell}";
        }
    }
}
