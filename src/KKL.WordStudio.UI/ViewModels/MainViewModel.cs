namespace KKL.WordStudio.UI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KKL.WordStudio.Application.Abstractions;
using KKL.WordStudio.Application.Workspace;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.UI.Services;
using Microsoft.Extensions.Logging;
using System.IO;

/// <summary>Shell-level ViewModel: owns the active Project and pushes it into IWorkspace, which every other panel (Project Explorer, Report Designer, Table Properties, Preview) reacts to.</summary>
public sealed partial class MainViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;
    private readonly IReportExporterRegistry _exporterRegistry;
    private readonly IWorkspace _workspace;
    private readonly IFileDialogService _fileDialogService;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    private Project _currentProject;

    [ObservableProperty]
    private string _statusText = "Ready";

    public MainViewModel(
        IProjectService projectService,
        IReportExporterRegistry exporterRegistry,
        IWorkspace workspace,
        IFileDialogService fileDialogService,
        ILogger<MainViewModel> logger)
    {
        _projectService = projectService;
        _exporterRegistry = exporterRegistry;
        _workspace = workspace;
        _fileDialogService = fileDialogService;
        _logger = logger;

        _currentProject = _projectService.CreateNew();
        _workspace.SetActiveProject(_currentProject);
        _workspace.SetActiveReport(_currentProject.Reports.FirstOrDefault());
    }

    [RelayCommand]
    private void NewProject()
    {
        CurrentProject = _projectService.CreateNew();
        _workspace.SetActiveProject(CurrentProject);
        _workspace.SetActiveReport(CurrentProject.Reports.FirstOrDefault());

        StatusText = $"Created '{CurrentProject.Name}'";
        _logger.LogInformation("New project created: {ProjectId}", CurrentProject.Id);
    }

    [RelayCommand]
    private async Task SaveProjectAsync()
    {
        var path = _fileDialogService.SaveProjectFile(CurrentProject.Name);
        if (path is null) return;

        var result = await _projectService.SaveAsync(CurrentProject, path);
        StatusText = result.IsSuccess ? $"Saved to '{path}'" : result.Error!;
    }

    [RelayCommand]
    private async Task ExportToWordAsync()
    {
        var report = _workspace.ActiveReport;
        if (report is null)
        {
            StatusText = "No active report to export";
            return;
        }

        var path = _fileDialogService.SaveWordFile(report.Name);
        if (path is null) return;

        var exporter = _exporterRegistry.Resolve("docx");
        var result = await exporter.ExportAsync(CurrentProject, report, ExportOptions.Default);

        if (result.IsFailure)
        {
            StatusText = result.Error!;
            _logger.LogWarning("Word export failed: {Error}", result.Error);
            return;
        }

        await using var fileStream = File.Create(path);
        await result.Value.CopyToAsync(fileStream);

        StatusText = $"Exported '{report.Name}' to '{path}'";
        _logger.LogInformation("Exported report {ReportId} to {Path}", report.Id, path);
    }
}
