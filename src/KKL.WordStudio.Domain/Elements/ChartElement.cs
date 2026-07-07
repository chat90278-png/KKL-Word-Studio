namespace KKL.WordStudio.Domain.Elements;

using KKL.WordStudio.Domain.Visitors;

public sealed class ChartElement : ReportElement
{
    public ChartType ChartType { get; set; } = ChartType.Bar;
    public string? DataRegionName { get; set; }

    public override void Accept(IReportElementVisitor visitor) => visitor.Visit(this);
}

public enum ChartType { Bar, Line, Pie, Area, Scatter }
