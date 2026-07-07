namespace KKL.WordStudio.Domain.Elements;

using KKL.WordStudio.Domain.Visitors;

public sealed class ShapeElement : ReportElement
{
    public ShapeType ShapeType { get; set; } = ShapeType.Rectangle;

    public override void Accept(IReportElementVisitor visitor) => visitor.Visit(this);
}

public enum ShapeType { Rectangle, Ellipse, Line }
