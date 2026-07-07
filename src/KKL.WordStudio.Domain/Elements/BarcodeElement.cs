namespace KKL.WordStudio.Domain.Elements;

using KKL.WordStudio.Domain.Expressions;
using KKL.WordStudio.Domain.Visitors;

public sealed class BarcodeElement : ReportElement
{
    public BarcodeType BarcodeType { get; set; } = BarcodeType.Code128;
    public Expression Value { get; set; } = Expression.Literal(string.Empty);
    public bool ShowLabel { get; set; } = true;

    public override void Accept(IReportElementVisitor visitor) => visitor.Visit(this);
}

public enum BarcodeType { Code128, Code39, Ean13, QrCode, DataMatrix, Pdf417 }
