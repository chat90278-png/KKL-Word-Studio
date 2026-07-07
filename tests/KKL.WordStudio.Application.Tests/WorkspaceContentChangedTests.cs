namespace KKL.WordStudio.Application.Tests;

using KKL.WordStudio.Application.Workspace;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;
using Xunit;

public class WorkspaceContentChangedTests
{
    [Fact]
    public void SelectingAnElement_DoesNotRaiseReportContentChanged()
    {
        var workspace = new Workspace();
        var project = new Project();
        var report = new Report();
        project.Reports.Add(report);
        workspace.SetActiveProject(project);
        workspace.SetActiveReport(report);

        var contentChangedCount = 0;
        workspace.ReportContentChanged += (_, _) => contentChangedCount++;

        workspace.SetSelectedReportElement(Guid.NewGuid());

        Assert.Equal(0, contentChangedCount);
    }

    [Fact]
    public void SwitchingActiveReport_RaisesReportContentChanged()
    {
        var workspace = new Workspace();
        var project = new Project();
        var reportA = new Report { Name = "A" };
        var reportB = new Report { Name = "B" };
        project.Reports.Add(reportA);
        project.Reports.Add(reportB);
        workspace.SetActiveProject(project);

        var contentChangedCount = 0;
        workspace.ReportContentChanged += (_, _) => contentChangedCount++;

        workspace.SetActiveReport(reportB);

        Assert.Equal(1, contentChangedCount);
    }

    [Fact]
    public void ExplicitNotify_RaisesReportContentChanged()
    {
        var workspace = new Workspace();
        var contentChangedCount = 0;
        workspace.ReportContentChanged += (_, _) => contentChangedCount++;

        workspace.NotifyReportContentChanged();

        Assert.Equal(1, contentChangedCount);
    }
}
