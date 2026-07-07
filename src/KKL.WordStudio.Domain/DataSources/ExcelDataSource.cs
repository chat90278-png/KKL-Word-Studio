namespace KKL.WordStudio.Domain.DataSources;

/// <summary>A data source backed by an imported Excel workbook.</summary>
public sealed class ExcelDataSource : DataSource
{
    public required Workbook Workbook { get; init; }

    /// <summary>Which worksheet within the workbook this data source reads from.</summary>
    public string? ActiveWorksheetName { get; set; }

    public override string ProviderKey => "excel";
}
