namespace KKL.WordStudio.UI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>Editable row backing a single ColumnMapping while the user reviews/adjusts auto-suggested field names before committing to the Domain model.</summary>
public sealed partial class ColumnMappingRowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _sourceColumn = string.Empty;

    [ObservableProperty]
    private string _fieldName = string.Empty;

    [ObservableProperty]
    private string _dataType = "string";
}
