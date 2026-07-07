namespace KKL.WordStudio.Domain.DataBinding;

/// <summary>A single field exposed by a data source (e.g., a column in a dataset).</summary>
public sealed class DataField
{
    public required string Name { get; init; }
    public required string DataType { get; init; }
}
