namespace KKL.WordStudio.Application.Preview;

using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;

/// <summary>
/// Produces a preview representation of a Report. Async and takes Project
/// (not just Report) as of Sprint 4, because resolving a bound table's
/// real rows requires the owning Project's DataSources — the exact same
/// requirement WordExporter has, since both now go through the same
/// IReportContentBuilder underneath.
/// </summary>
public interface IReportPreviewRenderer
{
    Task<PreviewSnapshot> RenderAsync(Project project, Report report, CancellationToken cancellationToken = default);
}
