namespace KKL.WordStudio.Domain.DataSources;

/// <summary>
/// Maps a raw spreadsheet column to a logical <see cref="DataBinding.DataField"/>.
/// This is where "Excel column ↔ report field" translation happens — once,
/// at the data-source level — so report elements only ever bind to logical
/// field names via Expression (e.g. "=Fields.CustomerName"), never to raw
/// column letters/indices.
/// </summary>
public sealed class ColumnMapping
{
    /// <summary>Either a column letter ("B") or a header name ("CustomerName"), depending on DataRange.HasHeaderRow.</summary>
    public required string SourceColumn { get; init; }
    public required Domain.DataBinding.DataField TargetField { get; init; }
}
