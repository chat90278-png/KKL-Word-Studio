namespace KKL.WordStudio.Application.Tests;

using KKL.WordStudio.Application.Abstractions;
using KKL.WordStudio.Application.Content;
using KKL.WordStudio.Application.Styling;
using KKL.WordStudio.Domain.DataBinding;
using KKL.WordStudio.Domain.DataSources;
using KKL.WordStudio.Domain.Elements;
using KKL.WordStudio.Domain.Expressions;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;
using KKL.WordStudio.Shared.Results;
using Xunit;

public class ReportContentBuilderTests
{
    private sealed class FakeDataProvider : IDataProvider
    {
        public string ProviderKey => "fake";

        public Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> GetRowsAsync(
            IDataSourceDefinition definition, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<IReadOnlyDictionary<string, object?>> rows = new List<IReadOnlyDictionary<string, object?>>
            {
                new Dictionary<string, object?> { ["CustomerName"] = "Bob", ["Amount"] = "200" },
                new Dictionary<string, object?> { ["CustomerName"] = "Alice", ["Amount"] = "100" },
            };
            return Task.FromResult(Result.Success(rows));
        }
    }

    private sealed class FakeDataProviderRegistry : IDataProviderRegistry
    {
        private readonly IDataProvider _provider = new FakeDataProvider();
        public void Register(IDataProvider provider) { }
        public IDataProvider Resolve(string providerKey) => _provider;
    }

    [Fact]
    public async Task Heading_And_AltHeading_AreClassifiedConsistently()
    {
        var report = new Report();
        var page = new Page();
        var section = new Section { Kind = SectionKind.Body };
        section.Root.Children.Add(new TextElement { Style = HeadingStylePresets.CreateHeadingStyle(), Content = Expression.Literal("Title") });
        section.Root.Children.Add(new TextElement { Style = HeadingStylePresets.CreateAltHeadingStyle(), Content = Expression.Literal("Subtitle") });
        section.Root.Children.Add(new TextElement { Content = Expression.Literal("Body text") });
        page.Sections.Add(section);
        report.Pages.Add(page);

        var project = new Project();
        project.Reports.Add(report);

        var builder = new ReportContentBuilder(new FakeDataProviderRegistry());
        var document = await builder.BuildAsync(project, report);

        Assert.Equal(ReportContentKind.Heading, document.BodyNodes[0].Kind);
        Assert.Equal(ReportContentKind.AltHeading, document.BodyNodes[1].Kind);
        Assert.Equal(ReportContentKind.Paragraph, document.BodyNodes[2].Kind);
    }

    [Fact]
    public async Task BoundTable_ResolvesRealRowsAndAppliesSortFields()
    {
        var dataSource = new ExcelDataSource
        {
            Name = "Sales",
            Workbook = new Workbook { FileName = "sales.xlsx" }
        };
        dataSource.ColumnMappings.Add(new ColumnMapping { SourceColumn = "A", TargetField = new DataField { Name = "CustomerName", DataType = "string" } });
        dataSource.ColumnMappings.Add(new ColumnMapping { SourceColumn = "B", TargetField = new DataField { Name = "Amount", DataType = "string" } });

        var project = new Project();
        project.DataSources.Add(dataSource);

        var report = new Report();
        var page = new Page();
        var section = new Section { Kind = SectionKind.Body };
        var table = new TableElement { Name = "SalesTable", Binding = new Binding { DataSourceName = "Sales" } };
        table.Binding.SortFields.Add(new SortField { FieldName = "CustomerName", Direction = SortDirection.Ascending });
        section.Root.Children.Add(table);
        page.Sections.Add(section);
        report.Pages.Add(page);

        var builder = new ReportContentBuilder(new FakeDataProviderRegistry());
        var document = await builder.BuildAsync(project, report);

        var tableNode = Assert.IsType<TableContentNode>(document.BodyNodes.Single());
        Assert.Equal("Sales", tableNode.DataSourceName);
        Assert.Equal(2, tableNode.Rows.Count);
        // FakeDataProvider returns Bob(200) then Alice(100); sorted ascending by CustomerName -> Alice first
        Assert.Equal("Alice", tableNode.Rows[0][0]);
        Assert.Equal("Bob", tableNode.Rows[1][0]);
    }

    [Fact]
    public async Task PageHeaderAndFooterSections_AreSeparatedFromBody()
    {
        var report = new Report();
        var page = new Page { ShowPageNumbers = true };
        var headerSection = new Section { Kind = SectionKind.PageHeader };
        var footerSection = new Section { Kind = SectionKind.PageFooter };
        var bodySection = new Section { Kind = SectionKind.Body };

        headerSection.Root.Children.Add(new TextElement { Content = Expression.Literal("Company Name") });
        footerSection.Root.Children.Add(new TextElement { Content = Expression.Literal("Confidential") });
        bodySection.Root.Children.Add(new TextElement { Content = Expression.Literal("Body paragraph") });

        page.Sections.Add(headerSection);
        page.Sections.Add(bodySection);
        page.Sections.Add(footerSection);
        report.Pages.Add(page);

        var project = new Project();
        project.Reports.Add(report);

        var builder = new ReportContentBuilder(new FakeDataProviderRegistry());
        var document = await builder.BuildAsync(project, report);

        Assert.Single(document.HeaderNodes);
        Assert.Single(document.FooterNodes);
        Assert.Single(document.BodyNodes);
        Assert.True(document.PageLayout.ShowPageNumbers);
    }

    [Fact]
    public async Task TableOfContents_IsDerivedFromHeadingsOnlyWhenEnabled()
    {
        var report = new Report { IncludeTableOfContents = true };
        var page = new Page();
        var section = new Section { Kind = SectionKind.Body };
        section.Root.Children.Add(new TextElement { Style = HeadingStylePresets.CreateHeadingStyle(), Content = Expression.Literal("Chapter 1") });
        section.Root.Children.Add(new TextElement { Style = HeadingStylePresets.CreateAltHeadingStyle(), Content = Expression.Literal("Section 1.1") });
        section.Root.Children.Add(new TextElement { Content = Expression.Literal("Some body text") });
        page.Sections.Add(section);
        report.Pages.Add(page);

        var project = new Project();
        project.Reports.Add(report);

        var builder = new ReportContentBuilder(new FakeDataProviderRegistry());
        var document = await builder.BuildAsync(project, report);

        Assert.Equal(2, document.TableOfContents.Count);
        Assert.Equal(1, document.TableOfContents[0].Level);
        Assert.Equal(2, document.TableOfContents[1].Level);
    }
}
