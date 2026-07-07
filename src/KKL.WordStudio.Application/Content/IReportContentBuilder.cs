namespace KKL.WordStudio.Application.Content;

using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;

/// <summary>
/// Builds the shared, format-agnostic interpretation of a Report — the one
/// piece of logic both the Preview renderer and WordExporter call. Neither
/// consumer walks the element tree itself or decides what counts as a
/// heading/table/TOC entry on its own; both ask this builder and get the
/// identical answer. Takes the owning Project (not just the Report)
/// because resolving a bound TableElement's actual rows requires looking
/// up its DataSource in Project.DataSources.
/// </summary>
public interface IReportContentBuilder
{
    Task<ReportContentDocument> BuildAsync(Project project, Report report, CancellationToken cancellationToken = default);
}
