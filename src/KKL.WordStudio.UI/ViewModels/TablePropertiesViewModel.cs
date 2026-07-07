namespace KKL.WordStudio.UI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KKL.WordStudio.Application.Workspace;
using KKL.WordStudio.Domain.DataBinding;
using KKL.WordStudio.Domain.DataSources;
using KKL.WordStudio.Domain.Elements;
using KKL.WordStudio.Domain.Visitors;

/// <summary>
/// Basic property + binding panel for whatever TableElement is currently
/// selected (Workspace.SelectedReportElementId). Deliberately basic per
/// Sprint 3 scope — Name, Description, a Bold/FontSize style pair, Show
/// Header, and Binding (DataSource picker + resolved Worksheet/DataRange
/// display). Filter/Sort editing is deferred to a later sprint.
///
/// The Worksheet/DataRange shown here are never stored on this panel or on
/// Binding itself — they're looked up live from the selected DataSource
/// (see ADR 0004), which is exactly what lets this panel exist without any
/// Domain change.
/// </summary>
public sealed partial class TablePropertiesViewModel : ViewModelBase
{
    private readonly IWorkspace _workspace;
    private TableElement? _selectedTable;

    public ObservableCollection<string> AvailableDataSourceNames { get; } = new();

    [ObservableProperty]
    private bool _hasSelection;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private bool _isBold;

    [ObservableProperty]
    private double _fontSize;

    [ObservableProperty]
    private bool _showHeader;

    [ObservableProperty]
    private string? _selectedDataSourceName;

    [ObservableProperty]
    private string _resolvedWorksheetDisplay = string.Empty;

    [ObservableProperty]
    private string _resolvedRangeDisplay = string.Empty;

    public TablePropertiesViewModel(IWorkspace workspace)
    {
        _workspace = workspace;
        _workspace.WorkspaceChanged += (_, _) => Refresh();
        Refresh();
    }

    private void Refresh()
    {
        AvailableDataSourceNames.Clear();
        foreach (var dataSource in _workspace.ActiveProject?.DataSources ?? Enumerable.Empty<DataSource>())
            AvailableDataSourceNames.Add(dataSource.Name);

        var report = _workspace.ActiveReport;
        var elementId = _workspace.SelectedReportElementId;

        _selectedTable = report is not null && elementId is not null
            ? ReportElementFlattener.FindById(report, elementId.Value) as TableElement
            : null;

        HasSelection = _selectedTable is not null;
        if (_selectedTable is null) return;

        Name = _selectedTable.Name;
        Description = _selectedTable.Description ?? string.Empty;
        IsBold = _selectedTable.Style.Bold;
        FontSize = _selectedTable.Style.FontSize;
        ShowHeader = _selectedTable.Rows.Any(r => r.Kind == TableRowKind.Header);
        SelectedDataSourceName = _selectedTable.Binding?.DataSourceName;

        RefreshResolvedBindingDisplay();
    }

    private void RefreshResolvedBindingDisplay()
    {
        ResolvedWorksheetDisplay = "—";
        ResolvedRangeDisplay = "—";

        var dataSourceName = _selectedTable?.Binding?.DataSourceName;
        if (dataSourceName is null) return;

        var dataSource = _workspace.ActiveProject?.DataSources
            .FirstOrDefault(ds => ds.Name == dataSourceName);

        if (dataSource is not ExcelDataSource excelDataSource) return;

        ResolvedWorksheetDisplay = excelDataSource.ActiveWorksheetName ?? "(no worksheet selected)";

        var worksheet = excelDataSource.Workbook.Worksheets
            .FirstOrDefault(w => w.Name == excelDataSource.ActiveWorksheetName);
        ResolvedRangeDisplay = worksheet?.SelectedRange?.RangeReference ?? "(no range configured)";
    }

    [RelayCommand]
    private void ApplyChanges()
    {
        if (_selectedTable is null) return;

        _selectedTable.Name = Name;
        _selectedTable.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;
        _selectedTable.Style.Bold = IsBold;
        _selectedTable.Style.FontSize = FontSize;

        ApplyShowHeaderToggle();
        _workspace.NotifyReportContentChanged();
    }

    private void ApplyShowHeaderToggle()
    {
        if (_selectedTable is null) return;

        var hasHeaderRow = _selectedTable.Rows.Any(r => r.Kind == TableRowKind.Header);

        if (ShowHeader && !hasHeaderRow)
        {
            _selectedTable.Rows.Insert(0, new TableRow { Kind = TableRowKind.Header });
        }
        else if (!ShowHeader && hasHeaderRow)
        {
            var headerRows = _selectedTable.Rows.Where(r => r.Kind == TableRowKind.Header).ToList();
            foreach (var row in headerRows)
                _selectedTable.Rows.Remove(row);
        }
    }

    [RelayCommand]
    private void ApplyBinding()
    {
        if (_selectedTable is null || SelectedDataSourceName is null) return;

        _selectedTable.Binding = new Binding { DataSourceName = SelectedDataSourceName };
        RefreshResolvedBindingDisplay();
        _workspace.NotifyReportContentChanged();
    }

    [RelayCommand]
    private void ClearBinding()
    {
        if (_selectedTable is null) return;

        _selectedTable.Binding = null;
        SelectedDataSourceName = null;
        RefreshResolvedBindingDisplay();
        _workspace.NotifyReportContentChanged();
    }
}
