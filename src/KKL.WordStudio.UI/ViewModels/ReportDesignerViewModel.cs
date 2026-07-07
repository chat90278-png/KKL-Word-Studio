namespace KKL.WordStudio.UI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KKL.WordStudio.Application.Styling;
using KKL.WordStudio.Application.Workspace;
using KKL.WordStudio.Domain.Elements;
using KKL.WordStudio.Domain.Expressions;
using KKL.WordStudio.Domain.Reports;

/// <summary>
/// Drives the first working Report Designer: shows the active Report as a
/// tree and lets the user add Heading / Alt Heading / Table elements.
///
/// Every "add" command funnels through <see cref="InsertElement"/> — the
/// single place that actually mutates the report tree. This is what makes
/// the design DnD-ready without implementing DnD today: a future drag-drop
/// handler calls the exact same method a button click calls now, so
/// "where does the insert logic live" never needs to be re-decided later.
///
/// Heading vs Alt Heading are NOT separate Domain types (see ADR 0003) —
/// both are TextElement with a different Style, using the shared
/// HeadingStylePresets convention so the Preview renderer classifies them
/// the same way this ViewModel creates them.
/// </summary>
public sealed partial class ReportDesignerViewModel : ViewModelBase
{
    private readonly IWorkspace _workspace;

    public ObservableCollection<ProjectExplorerNodeViewModel> ReportTreeNodes { get; } = new();

    [ObservableProperty]
    private string? _activeReportName;

    [ObservableProperty]
    private bool _includeTableOfContents;

    public ReportDesignerViewModel(IWorkspace workspace)
    {
        _workspace = workspace;
        _workspace.WorkspaceChanged += (_, _) => Rebuild();
        Rebuild();
    }

    [RelayCommand]
    private void AddHeading() => InsertElement(new TextElement
    {
        Name = "Heading",
        Style = HeadingStylePresets.CreateHeadingStyle(),
        Content = Expression.Literal("New heading")
    });

    [RelayCommand]
    private void AddAltHeading() => InsertElement(new TextElement
    {
        Name = "Alt Heading",
        Style = HeadingStylePresets.CreateAltHeadingStyle(),
        Content = Expression.Literal("New alt heading")
    });

    [RelayCommand]
    private void AddTable()
    {
        var table = new TableElement { Name = "Table" };
        table.Columns.Add(new TableColumn { Header = "Column 1" });
        table.Columns.Add(new TableColumn { Header = "Column 2" });
        table.Rows.Add(new TableRow { Kind = TableRowKind.Header });
        table.Rows.Add(new TableRow { Kind = TableRowKind.Detail });
        InsertElement(table);
    }

    [RelayCommand]
    private void AddHeaderText() => InsertElement(
        new TextElement { Name = "Header Text", Content = Expression.Literal("Header text") },
        SectionKind.PageHeader);

    [RelayCommand]
    private void AddFooterText() => InsertElement(
        new TextElement { Name = "Footer Text", Content = Expression.Literal("Footer text") },
        SectionKind.PageFooter);

    partial void OnIncludeTableOfContentsChanged(bool value)
    {
        var report = _workspace.ActiveReport;
        if (report is null) return;

        report.IncludeTableOfContents = value;
        _workspace.NotifyReportContentChanged();
    }

    /// <summary>The one place that inserts a new element into the active report's target container. Buttons call this today; a future DnD handler calls it too.</summary>
    private void InsertElement(ReportElement element, SectionKind targetKind = SectionKind.Body)
    {
        var report = _workspace.ActiveReport;
        if (report is null) return;

        var targetSection = report.Pages
            .SelectMany(p => p.Sections)
            .FirstOrDefault(s => s.Kind == targetKind);

        if (targetSection is null)
        {
            // Header/Footer sections aren't created by default (only Body is,
            // in KwsProjectRepository.CreateNew) — create one on first use
            // rather than requiring the user to somehow add it beforehand.
            var page = report.Pages.FirstOrDefault();
            if (page is null) return;

            targetSection = new Section
            {
                Name = targetKind.ToString(),
                Kind = targetKind,
                AutoHeight = targetKind == SectionKind.Body
            };
            page.Sections.Add(targetSection);
        }

        targetSection.Root.Children.Add(element);
        _workspace.SetSelectedReportElement(element.Id);
        _workspace.NotifyReportContentChanged();
        Rebuild();
    }

    private void Rebuild()
    {
        ReportTreeNodes.Clear();
        var report = _workspace.ActiveReport;
        ActiveReportName = report?.Name;
        if (report is null) return;

        IncludeTableOfContents = report.IncludeTableOfContents;

        foreach (var page in report.Pages)
        {
            foreach (var section in page.Sections)
            {
                var sectionNode = new ProjectExplorerNodeViewModel { Name = $"{section.Kind} ({section.Name})" };
                foreach (var child in section.Root.Children)
                    sectionNode.Children.Add(BuildNode(child));
                ReportTreeNodes.Add(sectionNode);
            }
        }
    }

    private ProjectExplorerNodeViewModel BuildNode(ReportElement element)
    {
        var name = element switch
        {
            TextElement t when HeadingStylePresets.IsHeading(t.Style) => $"Heading: {t.Content.Text}",
            TextElement t when HeadingStylePresets.IsAltHeading(t.Style) => $"Alt Heading: {t.Content.Text}",
            TextElement t => $"Text: {t.Content.Text}",
            TableElement tbl => $"Table: {tbl.Name}",
            Container => "Container",
            DataRegion dr => $"Data Region: {dr.Name}",
            _ => element.Name
        };

        var node = new ProjectExplorerNodeViewModel
        {
            Name = name,
            OnSelected = () => _workspace.SetSelectedReportElement(element.Id)
        };

        switch (element)
        {
            case Container container:
                foreach (var child in container.Children)
                    node.Children.Add(BuildNode(child));
                break;
            case TableElement table:
                foreach (var row in table.Rows)
                    foreach (var cell in row.Cells)
                        node.Children.Add(BuildNode(cell));
                break;
            case DataRegion dataRegion:
                node.Children.Add(BuildNode(dataRegion.Template));
                break;
        }

        return node;
    }
}
