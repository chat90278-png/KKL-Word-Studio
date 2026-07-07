namespace KKL.WordStudio.UI.ViewModels;

using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KKL.WordStudio.Application.Preview;
using KKL.WordStudio.Application.Workspace;

/// <summary>
/// Drives the Sprint 5 "real A4 preview": a single simulated page sized
/// and margined from the Report's actual Page dimensions, with distinct
/// Header/Body/Footer regions, an optional table of contents, and zoom.
///
/// Deliberately renders ONE page, not true multi-page pagination — page
/// count/overflow-splitting is real layout/pagination work that belongs to
/// the future Engine (ADR 0002/0005), not to this preview. The page shape,
/// margins, header/footer content, and TOC are real; where content would
/// actually break across pages in Word is not simulated here.
///
/// Subscribes to Workspace.ReportContentChanged (not the broader
/// WorkspaceChanged) — see ADR 0007 — so merely selecting a tree node
/// elsewhere no longer forces bound tables' Excel files to be re-read.
/// </summary>
public sealed partial class PreviewViewModel : ViewModelBase
{
    private const double MillimetersToPixels = 96.0 / 25.4; // WPF device-independent pixels at 96 DPI

    private readonly IWorkspace _workspace;
    private readonly IReportPreviewRenderer _renderer;

    public ObservableCollection<PreviewBlockViewModel> HeaderBlocks { get; } = new();
    public ObservableCollection<PreviewBlockViewModel> BodyBlocks { get; } = new();
    public ObservableCollection<PreviewBlockViewModel> FooterBlocks { get; } = new();
    public ObservableCollection<TocEntryViewModel> TableOfContents { get; } = new();

    [ObservableProperty] private double _pageWidth = 210 * MillimetersToPixels;
    [ObservableProperty] private double _pageHeight = 297 * MillimetersToPixels;
    [ObservableProperty] private Thickness _pageMargin = new(20 * MillimetersToPixels);
    [ObservableProperty] private bool _showPageNumber = true;
    [ObservableProperty] private bool _hasTableOfContents;
    [ObservableProperty] private double _zoomFactor = 1.0;

    public PreviewViewModel(IWorkspace workspace, IReportPreviewRenderer renderer)
    {
        _workspace = workspace;
        _renderer = renderer;
        _workspace.ReportContentChanged += (_, _) => _ = RefreshAsync();
        _ = RefreshAsync();
    }

    [RelayCommand]
    private void ZoomIn() => ZoomFactor = Math.Min(2.0, Math.Round(ZoomFactor + 0.1, 2));

    [RelayCommand]
    private void ZoomOut() => ZoomFactor = Math.Max(0.25, Math.Round(ZoomFactor - 0.1, 2));

    [RelayCommand]
    private void ResetZoom() => ZoomFactor = 1.0;

    private async Task RefreshAsync()
    {
        var project = _workspace.ActiveProject;
        var report = _workspace.ActiveReport;
        if (project is null || report is null)
        {
            HeaderBlocks.Clear();
            BodyBlocks.Clear();
            FooterBlocks.Clear();
            TableOfContents.Clear();
            return;
        }

        var snapshot = await _renderer.RenderAsync(project, report);

        PageWidth = snapshot.PageLayout.WidthMillimeters * MillimetersToPixels;
        PageHeight = snapshot.PageLayout.HeightMillimeters * MillimetersToPixels;
        PageMargin = new Thickness(
            snapshot.PageLayout.MarginLeftMillimeters * MillimetersToPixels,
            snapshot.PageLayout.MarginTopMillimeters * MillimetersToPixels,
            snapshot.PageLayout.MarginRightMillimeters * MillimetersToPixels,
            snapshot.PageLayout.MarginBottomMillimeters * MillimetersToPixels);
        ShowPageNumber = snapshot.PageLayout.ShowPageNumbers;

        HeaderBlocks.Clear();
        foreach (var block in snapshot.HeaderBlocks) HeaderBlocks.Add(Map(block));

        BodyBlocks.Clear();
        foreach (var block in snapshot.BodyBlocks) BodyBlocks.Add(Map(block));

        FooterBlocks.Clear();
        foreach (var block in snapshot.FooterBlocks) FooterBlocks.Add(Map(block));

        TableOfContents.Clear();
        foreach (var entry in snapshot.TableOfContents)
            TableOfContents.Add(new TocEntryViewModel { Text = entry.Text, Level = entry.Level });
        HasTableOfContents = TableOfContents.Count > 0;
    }

    private static PreviewBlockViewModel Map(PreviewBlock block) =>
        new() { ElementId = block.ElementId, Kind = block.Kind, Text = block.Text };
}
