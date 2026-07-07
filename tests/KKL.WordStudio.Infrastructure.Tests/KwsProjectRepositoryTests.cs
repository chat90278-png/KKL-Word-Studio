namespace KKL.WordStudio.Infrastructure.Tests;

using KKL.WordStudio.Domain.DataBinding;
using KKL.WordStudio.Domain.DataSources;
using KKL.WordStudio.Domain.Elements;
using KKL.WordStudio.Domain.Expressions;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;
using KKL.WordStudio.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class KwsProjectRepositoryTests
{
    [Fact]
    public async Task SaveThenOpen_RoundTripsProjectAndReportNames()
    {
        var repository = new KwsProjectRepository(NullLogger<KwsProjectRepository>.Instance);
        var project = new Project { Name = "Quarterly Sales Project" };
        var report = new Report { Name = "Quarterly Sales Report" };
        report.Pages.Add(new Page());
        project.Reports.Add(report);

        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        var saveResult = await repository.SaveAsync(project, tempFile);
        Assert.True(saveResult.IsSuccess);

        var savedPath = tempFile + ".kws";
        var openResult = await repository.OpenAsync(savedPath);

        Assert.True(openResult.IsSuccess);
        Assert.Equal("Quarterly Sales Project", openResult.Value.Name);
        var openedReport = Assert.Single(openResult.Value.Reports);
        Assert.Equal("Quarterly Sales Report", openedReport.Name);
        Assert.Single(openedReport.Pages);

        File.Delete(savedPath);
    }

    [Fact]
    public async Task SaveThenOpen_RoundTripsProjectAggregateGraph()
    {
        var repository = new KwsProjectRepository(NullLogger<KwsProjectRepository>.Instance);
        var project = new Project { Name = "Quarterly Sales Project" };

        var dataSource = new ExcelDataSource
        {
            Name = "SalesWorkbook",
            Workbook = new Workbook { FileName = "sales.xlsx", SourcePath = @"C:\Data\sales.xlsx" },
            ActiveWorksheetName = "Sales"
        };
        dataSource.Workbook.Worksheets.Add(new Worksheet
        {
            Name = "Sales",
            SelectedRange = new DataRange
            {
                DataStartRow = 2,
                DataEndRow = 10,
                HeaderRowIndex = 1,
                StartColumn = 1,
                EndColumn = 3,
                WasAutoDetected = true
            }
        });
        dataSource.ColumnMappings.Add(new ColumnMapping
        {
            SourceColumn = "A",
            TargetField = new DataField { Name = "Region", DataType = "Text" }
        });
        project.DataSources.Add(dataSource);

        var report = new Report { Name = "Quarterly Sales Report" };
        var page = new Page { Name = "Main" };
        var section = new Section { Name = "Body", Kind = SectionKind.Body };
        section.Root.Children.Add(new TextElement
        {
            Name = "Title",
            Content = Expression.Literal("Quarterly Sales")
        });

        var table = new TableElement
        {
            Name = "SalesTable",
            Binding = new Binding
            {
                DataSourceName = "SalesWorkbook",
                Filter = new Expression { Text = "=Fields.Region <> ''", ResultType = ExpressionResultType.Boolean }
            }
        };
        table.Columns.Add(new TableColumn { Header = "Region", Width = 75 });
        table.Rows.Add(new TableRow());
        table.Rows[0].Cells.Add(new Container());
        table.Rows[0].Cells[0].Children.Add(new TextElement
        {
            Content = new Expression { Text = "=Fields.Region", ResultType = ExpressionResultType.Text }
        });
        table.Binding.SortFields.Add(new SortField { FieldName = "Region", Direction = SortDirection.Descending });
        section.Root.Children.Add(table);

        page.Sections.Add(section);
        report.Pages.Add(page);
        project.Reports.Add(report);

        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var saveResult = await repository.SaveAsync(project, tempFile);
        Assert.True(saveResult.IsSuccess);

        var savedPath = tempFile + ".kws";
        var openResult = await repository.OpenAsync(savedPath);

        Assert.True(openResult.IsSuccess);
        var openedProject = openResult.Value;

        var openedDataSource = Assert.IsType<ExcelDataSource>(Assert.Single(openedProject.DataSources));
        Assert.Equal("SalesWorkbook", openedDataSource.Name);
        Assert.Equal("sales.xlsx", openedDataSource.Workbook.FileName);
        Assert.Equal("Sales", openedDataSource.ActiveWorksheetName);
        Assert.Equal("Region", Assert.Single(openedDataSource.ColumnMappings).TargetField.Name);
        var openedRange = Assert.Single(openedDataSource.Workbook.Worksheets).SelectedRange!;
        Assert.Equal(1, openedRange.HeaderRowIndex);
        Assert.Equal(2, openedRange.DataStartRow);
        Assert.Equal(10, openedRange.DataEndRow);
        Assert.Equal("A2:C10", openedRange.RangeReference);

        var openedReport = Assert.Single(openedProject.Reports);
        var openedPage = Assert.Single(openedReport.Pages);
        var openedSection = Assert.Single(openedPage.Sections);
        Assert.Equal("Body", openedSection.Name);

        var openedTitle = Assert.IsType<TextElement>(openedSection.Root.Children[0]);
        Assert.Equal("Quarterly Sales", openedTitle.Content.Text);

        var openedTable = Assert.IsType<TableElement>(openedSection.Root.Children[1]);
        Assert.Equal("SalesTable", openedTable.Name);
        Assert.Equal("Region", Assert.Single(openedTable.Columns).Header);
        Assert.Equal("SalesWorkbook", openedTable.Binding!.DataSourceName);
        Assert.Equal("=Fields.Region <> ''", openedTable.Binding.Filter!.Text);
        Assert.Equal(SortDirection.Descending, Assert.Single(openedTable.Binding.SortFields).Direction);

        var openedCellText = Assert.IsType<TextElement>(Assert.Single(Assert.Single(openedTable.Rows).Cells).Children.Single());
        Assert.Equal("=Fields.Region", openedCellText.Content.Text);

        File.Delete(savedPath);
    }
}
