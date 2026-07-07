namespace KKL.WordStudio.Domain.DataSources;

/// <summary>Structural description of an Excel file: which sheets it has. The Domain never opens the file itself — reading actual cell values is an Application/Infrastructure concern (IDataProvider).</summary>
public sealed class Workbook
{
    public required string FileName { get; init; }

    /// <summary>
    /// Full path used to originally open this workbook. Added in Sprint 4:
    /// exporting a bound table needs to re-read the actual file, and
    /// FileName alone (display-only) isn't enough to locate it. Nullable
    /// because a Workbook reconstructed purely from a saved .kws for
    /// display purposes (e.g. before the user re-links the source file)
    /// may not have one yet.
    /// </summary>
    public string? SourcePath { get; set; }

    public List<Worksheet> Worksheets { get; } = new();
}
