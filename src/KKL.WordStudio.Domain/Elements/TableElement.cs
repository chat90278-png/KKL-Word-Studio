namespace KKL.WordStudio.Domain.Elements;

using KKL.WordStudio.Domain.DataBinding;
using KKL.WordStudio.Domain.Visitors;

public sealed class TableElement : ReportElement
{
    public List<TableColumn> Columns { get; } = new();
    public List<TableRow> Rows { get; } = new();

    /// <summary>Free-text notes for the report author — added in Sprint 3 for the Table Designer's basic property panel. Kept on TableElement specifically rather than promoted to ReportElement; promote only once a second element type genuinely needs it.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// When set, this table is a data-bound ReportTable: its Detail row(s)
    /// repeat once per row returned by the named DataSource, with cell
    /// content populated via each cell's Expression (e.g. "=Fields.Total").
    /// Null means the table is purely static/manually authored.
    /// </summary>
    public Binding? Binding { get; set; }

    public override void Accept(IReportElementVisitor visitor) => visitor.Visit(this);
}

public sealed class TableColumn
{
    public string Header { get; set; } = string.Empty;
    public double Width { get; set; } = 100;
}

public sealed class TableRow
{
    public List<Container> Cells { get; } = new();
    public TableRowKind Kind { get; set; } = TableRowKind.Detail;
}

public enum TableRowKind { Header, Detail, Footer, GroupHeader, GroupFooter }
