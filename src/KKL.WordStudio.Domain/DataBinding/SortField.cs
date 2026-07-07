namespace KKL.WordStudio.Domain.DataBinding;

/// <summary>A single sort clause used by Binding.SortFields. Structured (field + direction) rather than a free-text Expression, since sort is always "by this field, this direction" — a concrete shape an exporter/engine can apply directly without parsing an expression grammar.</summary>
public sealed class SortField
{
    public required string FieldName { get; init; }
    public SortDirection Direction { get; set; } = SortDirection.Ascending;
}

public enum SortDirection { Ascending, Descending }
