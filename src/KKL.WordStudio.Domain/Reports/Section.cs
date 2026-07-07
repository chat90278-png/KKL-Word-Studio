namespace KKL.WordStudio.Domain.Reports;

using KKL.WordStudio.Domain.Elements;

/// <summary>
/// A horizontal band within a Page (e.g., Header, Body, Footer, Group
/// Header/Footer) — the same concept used by Report Builder/FastReport.
/// A Section holds a single root Container, keeping the "one child
/// collection" rule consistent with every other grouping element.
/// </summary>
public sealed class Section
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public SectionKind Kind { get; set; } = SectionKind.Body;
    public double Height { get; set; } = 100;

    /// <summary>
    /// When true (default), this section grows to fit its content rather than
    /// clipping to Height. Classic banded report engines assume fixed-height
    /// bands; KKL Word Studio's usage — building a flowing document skeleton —
    /// needs sections that grow by default. Height remains meaningful for the
    /// rare fixed-size case (e.g. a footer that must be exactly 2cm), set
    /// AutoHeight = false for that case.
    /// </summary>
    public bool AutoHeight { get; set; } = true;

    public Container Root { get; set; } = new();
}

public enum SectionKind
{
    ReportHeader,
    PageHeader,
    Body,
    PageFooter,
    ReportFooter,
    GroupHeader,
    GroupFooter
}
