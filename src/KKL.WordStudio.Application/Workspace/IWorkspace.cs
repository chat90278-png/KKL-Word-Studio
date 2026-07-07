namespace KKL.WordStudio.Application.Workspace;

using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;

/// <summary>
/// Holds the current editing session's runtime state. Kept intentionally
/// lightweight — identifiers and flags only, never bulk data — so it stays
/// a cheap, cross-panel reference point rather than growing into a god
/// object.
///
/// Sprint 5 addition: <see cref="ReportContentChanged"/> is deliberately
/// separate from <see cref="WorkspaceChanged"/>. The broad event fires on
/// every selection/tab change too (harmless for panels that just re-walk
/// an in-memory tree), but Preview's rebuild re-reads bound tables'
/// backing Excel files — firing that on a mere tree-selection click was a
/// real, measured inefficiency (see ADR 0007). Only actions that actually
/// mutate report content, or switch which project/report is active, raise
/// ReportContentChanged.
/// </summary>
public interface IWorkspace
{
    Project? ActiveProject { get; }
    Report? ActiveReport { get; }

    string? ActiveDataSourceName { get; }
    string? SelectedWorksheetName { get; }

    Guid? SelectedReportElementId { get; }
    bool IsPreviewActive { get; }

    void SetActiveProject(Project? project);
    void SetActiveReport(Report? report);
    void SetActiveDataSource(string? dataSourceName, string? worksheetName);
    void SetSelectedReportElement(Guid? reportElementId);
    void SetPreviewActive(bool isActive);

    /// <summary>Called by editors (Report Designer, Table Properties) after actually mutating the active Report's content — never for selection-only changes.</summary>
    void NotifyReportContentChanged();

    event EventHandler? WorkspaceChanged;

    /// <summary>Narrower than WorkspaceChanged — fires only for actual content mutations or project/report switches. Preview subscribes to this instead of WorkspaceChanged to avoid re-resolving bound data on unrelated UI interactions.</summary>
    event EventHandler? ReportContentChanged;
}
