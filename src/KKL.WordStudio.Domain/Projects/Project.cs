namespace KKL.WordStudio.Domain.Projects;

using KKL.WordStudio.Domain.DataSources;
using KKL.WordStudio.Domain.Reports;

/// <summary>
/// The true aggregate root of the domain (see ADR 0003). A Project owns the
/// data sources imported into it and the report designs built against them.
/// A single Project can hold multiple Reports sharing the same DataSources
/// (e.g., a "Summary" and a "Detailed" report over the same workbook) — this
/// is why DataSources live here rather than on Report itself.
/// </summary>
public sealed class Project
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Untitled Project";

    public List<DataSource> DataSources { get; } = new();
    public List<Report> Reports { get; } = new();
    public ProjectSettings Settings { get; set; } = new();
}
