namespace KKL.WordStudio.UI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>Represents one currently-open Excel file in the Excel Workspace. Multiple instances of this can coexist, satisfying the "birden fazla Excel dosyası açılabilsin" requirement.</summary>
public sealed partial class OpenWorkbookViewModel : ViewModelBase
{
    public required string FilePath { get; init; }
    public required string DisplayName { get; init; }

    public ObservableCollection<string> SheetNames { get; } = new();

    [ObservableProperty]
    private string? _selectedSheetName;
}
