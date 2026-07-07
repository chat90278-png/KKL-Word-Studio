namespace KKL.WordStudio.Application.Content;

/// <summary>
/// Format-agnostic interpretation of one piece of report content, produced
/// once by ReportContentBuilder and consumed by both the Preview renderer
/// and WordExporter. Neither consumer re-derives "is this a heading" or
/// "what rows does this bound table actually have" — that work happens
/// exactly once, here.
/// </summary>
public abstract class ReportContentNode
{
    public required Guid ElementId { get; init; }
    public required ReportContentKind Kind { get; init; }
}

public sealed class TextContentNode : ReportContentNode
{
    public required string Text { get; init; }
    public bool Bold { get; init; }
    public double FontSize { get; init; }
}

public sealed class TableContentNode : ReportContentNode
{
    public required string Name { get; init; }
    public required IReadOnlyList<string> ColumnHeaders { get; init; }

    /// <summary>Resolved cell text, outer = rows, inner = columns (same order as ColumnHeaders). For a bound table, these came from IDataProvider + Binding.SortFields, applied once here.</summary>
    public required IReadOnlyList<IReadOnlyList<string>> Rows { get; init; }

    public string? DataSourceName { get; init; }

    /// <summary>True if this table is bound but its Filter could not be applied (no expression evaluator exists yet — see ADR 0006). Surfaced so consumers can show/log that rows are unfiltered rather than silently dropping the filter.</summary>
    public bool FilterWasIgnored { get; init; }
}

public sealed class ImageContentNode : ReportContentNode
{
    public required string Name { get; init; }
}
