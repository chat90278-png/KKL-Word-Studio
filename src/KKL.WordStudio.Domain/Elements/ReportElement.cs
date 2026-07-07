namespace KKL.WordStudio.Domain.Elements;

using System.Text.Json.Serialization;
using KKL.WordStudio.Domain.Styling;
using KKL.WordStudio.Domain.Visitors;

/// <summary>
/// Base type for every visual/structural element that can appear in a
/// report. Deliberately abstract and Word/PDF/HTML-agnostic — see
/// ADR 0001. Every concrete element must implement Accept() for the
/// visitor pattern so exporters and the renderer can traverse the tree
/// uniformly.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BarcodeElement), "barcode")]
[JsonDerivedType(typeof(ChartElement), "chart")]
[JsonDerivedType(typeof(Container), "container")]
[JsonDerivedType(typeof(DataRegion), "dataRegion")]
[JsonDerivedType(typeof(ImageElement), "image")]
[JsonDerivedType(typeof(ShapeElement), "shape")]
[JsonDerivedType(typeof(TableElement), "table")]
[JsonDerivedType(typeof(TextElement), "text")]
public abstract class ReportElement
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Layout Layout { get; set; } = new();
    public Style Style { get; set; } = new();

    /// <summary>Double-dispatch entry point for <see cref="IReportElementVisitor"/>.</summary>
    public abstract void Accept(IReportElementVisitor visitor);
}
