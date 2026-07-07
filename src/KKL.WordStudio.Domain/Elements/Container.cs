namespace KKL.WordStudio.Domain.Elements;

using KKL.WordStudio.Domain.Visitors;

/// <summary>
/// A generic grouping element that holds child elements. Sections,
/// TableElement cells, and arbitrary grouping boxes all reuse this rather
/// than each inventing their own child-collection semantics.
/// </summary>
public class Container : ReportElement
{
    public List<ReportElement> Children { get; } = new();

    public override void Accept(IReportElementVisitor visitor) => visitor.Visit(this);
}
