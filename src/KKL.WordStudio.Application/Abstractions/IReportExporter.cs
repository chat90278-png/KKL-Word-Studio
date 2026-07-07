namespace KKL.WordStudio.Application.Abstractions;

using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;
using KKL.WordStudio.Shared.Results;

/// <summary>
/// Application-level use case for turning a <see cref="Report"/> into an
/// output format. Deliberately NOT in Domain (see ADR 0002) — exporting is
/// something the application *does*, not a property of what a report *is*.
/// Concrete implementations (WordExporter, PdfExporter, ...) live in
/// Infrastructure and are resolved at runtime via
/// <see cref="IReportExporterRegistry"/>, so Application code never
/// hard-references a specific exporter type.
///
/// Takes the owning Project (not just Report) as of Sprint 4: exporting a
/// report with bound tables requires resolving those bindings against
/// Project.DataSources — the same requirement the Preview renderer has,
/// since both now share IReportContentBuilder underneath.
/// </summary>
public interface IReportExporter
{
    /// <summary>Stable key used for registry lookup and .kws "last export format" metadata, e.g. "docx", "pdf".</summary>
    string FormatKey { get; }

    string DisplayName { get; }

    Task<Result<Stream>> ExportAsync(Project project, Report report, ExportOptions options, CancellationToken cancellationToken = default);
}

public sealed class ExportOptions
{
    public static ExportOptions Default { get; } = new();
}
