namespace KKL.WordStudio.Application.Preview;

using KKL.WordStudio.Application.Content;

/// <summary>
/// A rendering-agnostic snapshot of a Report, built directly from
/// IReportContentBuilder's shared ReportContentDocument (Sprint 4/5) — the
/// same interpretation WordExporter consumes, mapped into UI-friendly
/// block lists per region instead of OpenXML. Reuses ReportContentKind and
/// the builder's own TOC/PageLayout so a block's Kind here and a node's
/// Kind used by the exporter can never disagree.
/// </summary>
public sealed class PreviewSnapshot
{
    public required IReadOnlyList<PreviewBlock> HeaderBlocks { get; init; }
    public required IReadOnlyList<PreviewBlock> BodyBlocks { get; init; }
    public required IReadOnlyList<PreviewBlock> FooterBlocks { get; init; }
    public required IReadOnlyList<TocEntry> TableOfContents { get; init; }
    public required PageLayout PageLayout { get; init; }
}

public sealed class PreviewBlock
{
    public required Guid ElementId { get; init; }
    public required ReportContentKind Kind { get; init; }
    public required string Text { get; init; }
}
