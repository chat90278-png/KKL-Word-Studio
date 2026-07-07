namespace KKL.WordStudio.Domain.Tests;

using KKL.WordStudio.Domain.Elements;
using KKL.WordStudio.Domain.Reports;
using KKL.WordStudio.Domain.Visitors;
using Xunit;

public class Sprint3DomainTests
{
    [Fact]
    public void Section_AutoHeight_DefaultsToTrue()
    {
        var section = new Section();
        Assert.True(section.AutoHeight);
    }

    [Fact]
    public void TableElement_CanCarryADescription()
    {
        var table = new TableElement { Description = "Quarterly sales by region" };
        Assert.Equal("Quarterly sales by region", table.Description);
    }

    [Fact]
    public void ReportElementFlattener_FindsNestedElementInsideTableCell()
    {
        var report = new Report();
        var page = new Page();
        var section = new Section { Kind = SectionKind.Body };
        page.Sections.Add(section);
        report.Pages.Add(page);

        var heading = new TextElement { Name = "Title" };
        section.Root.Children.Add(heading);

        var table = new TableElement { Name = "SalesTable" };
        var cellText = new TextElement { Name = "CellValue" };
        var cellContainer = new Container();
        cellContainer.Children.Add(cellText);
        table.Rows.Add(new TableRow { Kind = TableRowKind.Detail, Cells = { cellContainer } });
        section.Root.Children.Add(table);

        var found = ReportElementFlattener.FindById(report, cellText.Id);

        Assert.NotNull(found);
        Assert.Same(cellText, found);
    }

    [Fact]
    public void ReportElementFlattener_FlattensAcrossMultipleSections()
    {
        var report = new Report();
        var page = new Page();
        var headerSection = new Section { Kind = SectionKind.PageHeader };
        var bodySection = new Section { Kind = SectionKind.Body };
        headerSection.Root.Children.Add(new TextElement { Name = "Header text" });
        bodySection.Root.Children.Add(new TableElement { Name = "Table1" });
        page.Sections.Add(headerSection);
        page.Sections.Add(bodySection);
        report.Pages.Add(page);

        var flattened = ReportElementFlattener.Flatten(report);

        Assert.Contains(flattened, e => e is TextElement t && t.Name == "Header text");
        Assert.Contains(flattened, e => e is TableElement t && t.Name == "Table1");
    }
}
