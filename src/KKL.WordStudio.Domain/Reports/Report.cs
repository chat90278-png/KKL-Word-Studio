namespace KKL.WordStudio.Domain.Reports;

/// <summary>
/// A single report design: a tree of Pages/Sections/Elements. Consumed by
/// every <c>IReportExporter</c> implementation (Application/Infrastructure
/// layer) to produce DOCX/PDF/HTML/etc. — the Domain has no notion that any
/// of those output formats exist.
///
/// As of ADR 0003, Report is no longer the aggregate root: it is owned by
/// <see cref="Projects.Project"/>, which also owns the DataSources a
/// project's reports bind against. A single Project can contain multiple
/// Reports sharing the same DataSources (e.g. a summary and a detailed
/// report over the same workbook), which is why DataSources moved off
/// Report rather than being duplicated per report.
/// </summary>
public sealed class Report
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Untitled Report";
    public string? Description { get; set; }

    /// <summary>When true, both Preview and WordExporter include a table of contents generated from Heading/AltHeading content (see ADR 0007).</summary>
    public bool IncludeTableOfContents { get; set; }

    public List<Page> Pages { get; } = new();

    /// <summary>Named, reusable styles that elements can reference by key.</summary>
    public Dictionary<string, Styling.Style> NamedStyles { get; } = new();
}
