namespace KKL.WordStudio.UI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using KKL.WordStudio.Application.Workspace;
using KKL.WordStudio.Domain.DataSources;
using KKL.WordStudio.Domain.Reports;

/// <summary>
/// Projects the existing Project aggregate (DataSources, Reports, Settings)
/// into a tree — no Domain change was needed for this (see Sprint 3 ADR):
/// the shape the Project Explorer wants is already there, this ViewModel
/// just walks it. Rebuilds whenever the Workspace's active project changes.
/// </summary>
public sealed partial class ProjectExplorerViewModel : ViewModelBase
{
    private readonly IWorkspace _workspace;

    public ObservableCollection<ProjectExplorerNodeViewModel> RootNodes { get; } = new();

    public ProjectExplorerViewModel(IWorkspace workspace)
    {
        _workspace = workspace;
        _workspace.WorkspaceChanged += (_, _) => Rebuild();
        Rebuild();
    }

    private void Rebuild()
    {
        RootNodes.Clear();
        var project = _workspace.ActiveProject;
        if (project is null) return;

        var dataSourcesNode = new ProjectExplorerNodeViewModel { Name = "Data Sources" };
        foreach (var dataSource in project.DataSources)
        {
            var dataSourceNode = new ProjectExplorerNodeViewModel
            {
                Name = dataSource.Name,
                OnSelected = () => _workspace.SetActiveDataSource(dataSource.Name, null)
            };

            if (dataSource is ExcelDataSource excelDataSource)
            {
                foreach (var worksheet in excelDataSource.Workbook.Worksheets)
                {
                    dataSourceNode.Children.Add(new ProjectExplorerNodeViewModel
                    {
                        Name = worksheet.Name,
                        OnSelected = () => _workspace.SetActiveDataSource(dataSource.Name, worksheet.Name)
                    });
                }
            }

            dataSourcesNode.Children.Add(dataSourceNode);
        }
        RootNodes.Add(dataSourcesNode);

        var reportsNode = new ProjectExplorerNodeViewModel { Name = "Reports" };
        foreach (var report in project.Reports)
        {
            reportsNode.Children.Add(new ProjectExplorerNodeViewModel
            {
                Name = report.Name,
                OnSelected = () => _workspace.SetActiveReport(report)
            });
        }
        RootNodes.Add(reportsNode);

        // Templates has no backing Domain model yet (deliberately deferred — ADR 0003/0005).
        // Shown as an empty placeholder so the tree's eventual shape is visible without
        // fabricating data for a feature that doesn't exist.
        var templatesNode = new ProjectExplorerNodeViewModel { Name = "Templates" };
        templatesNode.Children.Add(new ProjectExplorerNodeViewModel { Name = "(none yet)" });
        RootNodes.Add(templatesNode);

        RootNodes.Add(new ProjectExplorerNodeViewModel { Name = $"Settings ({project.Settings.DefaultPageOrientation})" });
    }
}
