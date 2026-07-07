namespace KKL.WordStudio.Domain.Elements;

using KKL.WordStudio.Domain.Expressions;
using KKL.WordStudio.Domain.Visitors;

/// <summary>A static or bound text label (equivalent to a "Label"/"Textbox" in other report designers).</summary>
public sealed class TextElement : ReportElement
{
    /// <summary>Either literal text or a bound expression such as "=Fields.CustomerName".</summary>
    public Expression Content { get; set; } = Expression.Literal(string.Empty);
    public bool WrapText { get; set; } = true;

    public override void Accept(IReportElementVisitor visitor) => visitor.Visit(this);
}
