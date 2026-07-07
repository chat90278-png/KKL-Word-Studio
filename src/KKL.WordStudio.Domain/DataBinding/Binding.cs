namespace KKL.WordStudio.Domain.DataBinding;

using KKL.WordStudio.Domain.Expressions;

/// <summary>
/// Declares that a report element (TableElement, DataRegion) is bound to a
/// named DataSource within the owning Project, plus any per-element
/// filtering/sorting of that data source's rows.
///
/// Deliberately does NOT carry Worksheet, DataRange, HeaderRow, or
/// ColumnMapping (considered and rejected in Sprint 2): those are already
/// resolved once, at the DataSource level
/// (ExcelDataSource.ActiveWorksheetName, Worksheet.SelectedRange,
/// DataSource.ColumnMappings). Duplicating them here would create two
/// sources of truth that could disagree. Filter/Sort, by contrast, are
/// legitimately per-element — two tables can read the same DataSource but
/// want different subsets/ordering of its rows.
/// </summary>
public sealed class Binding
{
    public required string DataSourceName { get; set; }

    /// <summary>Optional boolean expression (e.g. "=Fields.Region = 'North'") narrowing which rows this element consumes.</summary>
    public Expression? Filter { get; set; }

    public List<SortField> SortFields { get; } = new();
}
