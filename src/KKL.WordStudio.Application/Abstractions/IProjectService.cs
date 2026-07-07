namespace KKL.WordStudio.Application.Abstractions;

using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Shared.Results;

/// <summary>
/// Use-case surface for opening/saving the native .kws project container.
/// Renamed from IReportProjectService (Sprint 1) to IProjectService after
/// ADR 0003 made Project, not Report, the aggregate root — this interface
/// now opens/saves a whole Project (data sources + reports + settings), not
/// a single Report. Implemented in Infrastructure (KwsProjectRepository);
/// the UI layer only ever talks to this interface, never to file format
/// details.
/// </summary>
public interface IProjectService
{
    Task<Result<Project>> OpenAsync(string filePath, CancellationToken cancellationToken = default);
    Task<Result> SaveAsync(Project project, string filePath, CancellationToken cancellationToken = default);
    Project CreateNew();
}
