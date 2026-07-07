namespace KKL.WordStudio.Domain.Visitors;

using KKL.WordStudio.Domain.Elements;
using KKL.WordStudio.Domain.Reports;

/// <summary>
/// Flattens a Report's element tree (walking Pages → Sections →
/// Container.Children, including nested containers, table rows/cells, and
/// DataRegion templates) into a single sequence. Added in Sprint 3 because
/// the Report Designer tree, the Table Properties panel, and the Preview
/// panel all independently needed "find the element behind this Guid" —
/// implementing it once here, on top of the existing visitor pattern,
/// avoids three separate ad-hoc tree walks that could drift out of sync
/// with each other as new element types are added.
/// </summary>
public sealed class ReportElementFlattener : IReportElementVisitor
{
    private readonly List<ReportElement> _elements = new();

    public static IReadOnlyList<ReportElement> Flatten(Report report)
    {
        var flattener = new ReportElementFlattener();
        foreach (var page in report.Pages)
            foreach (var section in page.Sections)
                section.Root.Accept(flattener);
        return flattener._elements;
    }

    public static ReportElement? FindById(Report report, Guid id) =>
        Flatten(report).FirstOrDefault(e => e.Id == id);

    public void Visit(TextElement element) => _elements.Add(element);
    public void Visit(ImageElement element) => _elements.Add(element);
    public void Visit(ShapeElement element) => _elements.Add(element);
    public void Visit(BarcodeElement element) => _elements.Add(element);
    public void Visit(ChartElement element) => _elements.Add(element);

    public void Visit(TableElement element)
    {
        _elements.Add(element);
        foreach (var row in element.Rows)
            foreach (var cell in row.Cells)
                cell.Accept(this);
    }

    public void Visit(DataRegion element)
    {
        _elements.Add(element);
        element.Template.Accept(this);
    }

    public void Visit(Container element)
    {
        _elements.Add(element);
        foreach (var child in element.Children)
            child.Accept(this);
    }
}
