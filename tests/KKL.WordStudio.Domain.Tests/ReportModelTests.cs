namespace KKL.WordStudio.Domain.Tests;

using KKL.WordStudio.Domain.Elements;
using KKL.WordStudio.Domain.Reports;
using Xunit;

public class ReportModelTests
{
    [Fact]
    public void NewReport_HasNoPagesByDefault()
    {
        var report = new Report();
        Assert.Empty(report.Pages);
    }

    [Fact]
    public void Section_Root_AcceptsChildElements()
    {
        var section = new Section { Kind = SectionKind.Body };
        section.Root.Children.Add(new TextElement { Name = "Title" });

        Assert.Single(section.Root.Children);
        Assert.IsType<TextElement>(section.Root.Children[0]);
    }

    [Fact]
    public void EveryElement_AcceptsVisitorWithoutThrowing()
    {
        var visitor = new CountingVisitor();
        ReportElement[] elements =
        {
            new TextElement(), new ImageElement(), new TableElement(),
            new ShapeElement(), new BarcodeElement(), new ChartElement(),
            new DataRegion(), new Container()
        };

        foreach (var element in elements)
            element.Accept(visitor);

        Assert.Equal(elements.Length, visitor.VisitCount);
    }

    private sealed class CountingVisitor : Visitors.IReportElementVisitor
    {
        public int VisitCount { get; private set; }
        public void Visit(TextElement element) => VisitCount++;
        public void Visit(ImageElement element) => VisitCount++;
        public void Visit(TableElement element) => VisitCount++;
        public void Visit(ShapeElement element) => VisitCount++;
        public void Visit(BarcodeElement element) => VisitCount++;
        public void Visit(ChartElement element) => VisitCount++;
        public void Visit(DataRegion element) => VisitCount++;
        public void Visit(Container element) => VisitCount++;
    }
}
