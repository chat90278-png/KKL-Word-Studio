namespace KKL.WordStudio.UI.ViewModels;

using System.Collections.ObjectModel;
using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KKL.WordStudio.Application.Excel;
using KKL.WordStudio.Application.Workspace;
using KKL.WordStudio.Domain.DataBinding;
using KKL.WordStudio.Domain.DataSources;
using KKL.WordStudio.UI.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Drives the Sprint 2 end-to-end flow: open Excel file(s) → pick sheet →
/// pick start row → auto-detect data end (with manual override) → map
/// columns → add the resulting ExcelDataSource to the active Project.
///
/// Deliberately holds the (potentially large) preview grid itself — see
/// ADR 0004: Workspace (Application layer) only tracks lightweight
/// identifiers/flags, so bulk preview data belongs here, in the panel that
/// actually renders it, not in the cross-cutting session singleton.
/// </summary>
public sealed partial class ExcelWorkspaceViewModel : ViewModelBase
{
    private readonly IExcelWorkbookReader _excelReader;
    private readonly IWorkspace _workspace;
    private readonly IFileDialogService _fileDialogService;
    private readonly ILogger<ExcelWorkspaceViewModel> _logger;

    private SheetPreview? _currentPreview;

    public ObservableCollection<OpenWorkbookViewModel> OpenWorkbooks { get; } = new();
    public ObservableCollection<ColumnMappingRowViewModel> ColumnMappings { get; } = new();

    [ObservableProperty]
    private OpenWorkbookViewModel? _selectedWorkbook;

    [ObservableProperty]
    private DataTable _previewTable = new();

    [ObservableProperty]
    private int _startRow = 1;

    [ObservableProperty]
    private bool _startRowIsHeader = true;

    [ObservableProperty]
    private int? _detectedDataEndRow;

    [ObservableProperty]
    private bool _wasAutoDetected;

    [ObservableProperty]
    private string _dataSourceName = string.Empty;

    [ObservableProperty]
    private string _statusText = string.Empty;

    public ExcelWorkspaceViewModel(
        IExcelWorkbookReader excelReader,
        IWorkspace workspace,
        IFileDialogService fileDialogService,
        ILogger<ExcelWorkspaceViewModel> logger)
    {
        _excelReader = excelReader;
        _workspace = workspace;
        _fileDialogService = fileDialogService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task OpenExcelFileAsync()
    {
        var filePath = _fileDialogService.OpenExcelFile();
        if (filePath is null) return;

        var result = await _excelReader.OpenWorkbookAsync(filePath);
        if (result.IsFailure)
        {
            StatusText = result.Error!;
            _logger.LogWarning("Failed to open workbook {FilePath}: {Error}", filePath, result.Error);
            return;
        }

        var workbookVm = new OpenWorkbookViewModel
        {
            FilePath = filePath,
            DisplayName = System.IO.Path.GetFileName(filePath)
        };
        foreach (var worksheet in result.Value.Worksheets)
            workbookVm.SheetNames.Add(worksheet.Name);

        OpenWorkbooks.Add(workbookVm);
        SelectedWorkbook = workbookVm;
        StatusText = $"Opened '{workbookVm.DisplayName}' with {workbookVm.SheetNames.Count} sheet(s)";

        if (workbookVm.SheetNames.Count > 0)
        {
            workbookVm.SelectedSheetName = workbookVm.SheetNames[0];
            await LoadPreviewAsync();
        }
    }

    [RelayCommand]
    private async Task LoadPreviewAsync()
    {
        if (SelectedWorkbook?.SelectedSheetName is null) return;

        var result = await _excelReader.GetSheetPreviewAsync(SelectedWorkbook.FilePath, SelectedWorkbook.SelectedSheetName);
        if (result.IsFailure)
        {
            StatusText = result.Error!;
            return;
        }

        _currentPreview = result.Value;
        PreviewTable = BuildPreviewTable(result.Value);
        DetectedDataEndRow = null;
        ColumnMappings.Clear();

        _workspace.SetActiveDataSource(DataSourceName, SelectedWorkbook.SelectedSheetName);
        _workspace.SetPreviewActive(true);

        StatusText = result.Value.IsTruncated
            ? $"Preview truncated to {result.Value.Rows.Count} rows"
            : $"Loaded {result.Value.Rows.Count} rows";
    }

    [RelayCommand]
    private async Task DetectDataRangeAsync()
    {
        if (SelectedWorkbook?.SelectedSheetName is null) return;

        var effectiveDataStart = StartRowIsHeader ? StartRow + 1 : StartRow;
        var result = await _excelReader.DetectDataRangeAsync(SelectedWorkbook.FilePath, SelectedWorkbook.SelectedSheetName, effectiveDataStart);

        if (result.IsFailure)
        {
            StatusText = result.Error!;
            return;
        }

        DetectedDataEndRow = result.Value.DataEndRow;
        WasAutoDetected = true;
        StatusText = $"Detected data range: {result.Value.RangeReference}";
    }

    /// <summary>Called when the user edits the detected end row directly in the UI — records that the value is no longer the system's guess (see DataRange.WasAutoDetected).</summary>
    partial void OnDetectedDataEndRowChanged(int? value)
    {
        WasAutoDetected = false;
    }

    [RelayCommand]
    private void GenerateColumnMappings()
    {
        if (_currentPreview is null) return;

        ColumnMappings.Clear();

        var headerRowValues = StartRowIsHeader
            ? _currentPreview.Rows.ElementAtOrDefault(_currentPreview.RowNumbers.ToList().IndexOf(StartRow))
            : null;

        for (var columnIndex = 0; columnIndex < _currentPreview.ColumnCount; columnIndex++)
        {
            var columnLetter = Shared.Spreadsheet.ColumnLetterConverter.ToLetters(columnIndex + 1);
            var suggestedName = headerRowValues is not null && columnIndex < headerRowValues.Count && !string.IsNullOrWhiteSpace(headerRowValues[columnIndex])
                ? headerRowValues[columnIndex]
                : $"Column{columnIndex + 1}";

            ColumnMappings.Add(new ColumnMappingRowViewModel
            {
                SourceColumn = columnLetter,
                FieldName = suggestedName,
                DataType = "string"
            });
        }

        StatusText = $"Generated {ColumnMappings.Count} column mapping(s) — review and adjust before saving";
    }

    [RelayCommand]
    private void AddDataSourceToProject()
    {
        var project = _workspace.ActiveProject;
        if (project is null)
        {
            StatusText = "No active project — create or open a project first";
            return;
        }
        if (SelectedWorkbook?.SelectedSheetName is not { } selectedSheetName)
        {
            StatusText = "Select a workbook and sheet first";
            return;
        }
        if (string.IsNullOrWhiteSpace(DataSourceName))
        {
            StatusText = "Enter a name for the data source";
            return;
        }

        var effectiveDataStart = StartRowIsHeader ? StartRow + 1 : StartRow;

        var worksheet = new Worksheet
        {
            Name = selectedSheetName,
            SelectedRange = new DataRange
            {
                DataStartRow = effectiveDataStart,
                DataEndRow = DetectedDataEndRow,
                HeaderRowIndex = StartRowIsHeader ? StartRow : null,
                WasAutoDetected = WasAutoDetected
            }
        };

        var dataSource = new ExcelDataSource
        {
            Name = DataSourceName,
            Workbook = new Workbook
            {
                FileName = System.IO.Path.GetFileName(SelectedWorkbook.FilePath),
                SourcePath = SelectedWorkbook.FilePath
            },
            ActiveWorksheetName = worksheet.Name
        };
        dataSource.Workbook.Worksheets.Add(worksheet);

        foreach (var mappingRow in ColumnMappings)
        {
            dataSource.ColumnMappings.Add(new ColumnMapping
            {
                SourceColumn = mappingRow.SourceColumn,
                TargetField = new Domain.DataBinding.DataField
                {
                    Name = mappingRow.FieldName,
                    DataType = mappingRow.DataType
                }
            });
        }

        project.DataSources.Add(dataSource);
        _workspace.SetActiveDataSource(dataSource.Name, worksheet.Name);

        StatusText = $"Added data source '{dataSource.Name}' ({dataSource.ColumnMappings.Count} fields) to project '{project.Name}'";
    }

    private static DataTable BuildPreviewTable(SheetPreview preview)
    {
        var table = new DataTable();
        table.Columns.Add("#"); // row-number column, satisfies "row headers shown"

        for (var i = 0; i < preview.ColumnCount; i++)
            table.Columns.Add(Shared.Spreadsheet.ColumnLetterConverter.ToLetters(i + 1));

        for (var rowIndex = 0; rowIndex < preview.Rows.Count; rowIndex++)
        {
            var dataRow = table.NewRow();
            dataRow[0] = preview.RowNumbers[rowIndex];

            var cells = preview.Rows[rowIndex];
            for (var columnIndex = 0; columnIndex < cells.Count; columnIndex++)
                dataRow[columnIndex + 1] = cells[columnIndex];

            table.Rows.Add(dataRow);
        }

        return table;
    }

    partial void OnSelectedWorkbookChanged(OpenWorkbookViewModel? value)
    {
        if (value is not null)
            _ = LoadPreviewAsync();
    }
}
