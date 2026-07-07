namespace KKL.WordStudio.Domain.Visitors;

using KKL.WordStudio.Domain.Elements;

/// <summary>
/// Visitor over the report element tree. Both the future Rendering engine
/// and any Application-layer exporter walk the same model through this
/// interface — neither needs type-switches over every element kind, and
/// adding a new element type only requires implementing one new Visit
/// overload per existing visitor (compiler enforces it).
/// </summary>
public interface IReportElementVisitor
{
    void Visit(TextElement element);
    void Visit(ImageElement element);
    void Visit(TableElement element);
    void Visit(ShapeElement element);
    void Visit(BarcodeElement element);
    void Visit(ChartElement element);
    void Visit(DataRegion element);
    void Visit(Container element);
}
