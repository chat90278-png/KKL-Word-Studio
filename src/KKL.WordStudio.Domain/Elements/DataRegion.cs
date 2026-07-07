namespace KKL.WordStudio.Domain.Elements;

using KKL.WordStudio.Domain.DataBinding;
using KKL.WordStudio.Domain.Visitors;

/// <summary>
/// A repeating region bound to a data source (the report-designer
/// equivalent of a "detail band" / list / repeater, used for non-tabular
/// repeated layouts — a TableElement is the equivalent construct when the
/// repeated content should render as a grid). Holds a template Container
/// that is repeated once per data row at export/render time — the *how* of
/// that repetition is an execution concern (future Engine), not something
/// Domain implements.
/// </summary>
public sealed class DataRegion : ReportElement
{
    /// <summary>Uses the same Binding type as TableElement (see DataBinding.Binding) so both repeating constructs share one binding model.</summary>
    public Binding? Binding { get; set; }
    public Container Template { get; set; } = new();

    public override void Accept(IReportElementVisitor visitor) => visitor.Visit(this);
}
