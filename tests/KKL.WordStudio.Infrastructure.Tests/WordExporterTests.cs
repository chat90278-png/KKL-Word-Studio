namespace KKL.WordStudio.Infrastructure.Tests;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using KKL.WordStudio.Application.Abstractions;
using KKL.WordStudio.Application.Content;
using KKL.WordStudio.Application.Styling;
using KKL.WordStudio.Domain.Elements;
using KKL.WordStudio.Domain.Expressions;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;
using KKL.WordStudio.Infrastructure.Export.Exporters;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class WordExporterTests
{
    private sealed class NoOpDataProviderRegistry : IDataProviderRegistry
    {
        public void Register(IDataProvider provider) { }
        public IDataProvider Resolve(string providerKey) => throw new InvalidOperationException("No bound tables in this test.");
    }

    [Fact]
    public async Task ExportAsync_ProducesValidDocxContainingHeadingAndTableText()
    {
        var report = new Report { Name = "Test Report" };
        var page = new Page();
        var section = new Section { Kind = SectionKind.Body };

        section.Root.Children.Add(new TextElement
        {
            Style = HeadingStylePresets.CreateHeadingStyle(),
            Content = Expression.Literal("Quarterly Sales")
        });

        var table = new TableElement { Name = "StaticTable" };
        table.Columns.Add(new TableColumn { Header = "Region" });
        table.Columns.Add(new TableColumn { Header = "Total" });
        section.Root.Children.Add(table);

        page.Sections.Add(section);
        report.Pages.Add(page);

        var project = new Project();
        project.Reports.Add(report);

        var contentBuilder = new ReportContentBuilder(new NoOpDataProviderRegistry());
        var exporter = new WordExporter(contentBuilder, NullLogger<WordExporter>.Instance);

        var result = await exporter.ExportAsync(project, report, ExportOptions.Default);

        Assert.True(result.IsSuccess);

        using var document = WordprocessingDocument.Open(result.Value, false);
        var body = document.MainDocumentPart!.Document.Body!;

        var allText = string.Join(" ", body.Descendants<Text>().Select(t => t.Text));
        Assert.Contains("Quarterly Sales", allText);
        Assert.Contains("Region", allText);
        Assert.Contains("Total", allText);

        Assert.Single(body.Descendants<Table>());
    }

    [Fact]
    public async Task ExportAsync_IncludesHeaderFooterPageNumberAndTocFields()
    {
        var report = new Report { Name = "Full Report", IncludeTableOfContents = true };
        var page = new Page { ShowPageNumbers = true };

        var headerSection = new Section { Kind = SectionKind.PageHeader };
        headerSection.Root.Children.Add(new TextElement { Content = Expression.Literal("Acme Corp") });

        var footerSection = new Section { Kind = SectionKind.PageFooter };
        footerSection.Root.Children.Add(new TextElement { Content = Expression.Literal("Confidential") });

        var bodySection = new Section { Kind = SectionKind.Body };
        bodySection.Root.Children.Add(new TextElement
        {
            Style = HeadingStylePresets.CreateHeadingStyle(),
            Content = Expression.Literal("Chapter One")
        });

        page.Sections.Add(headerSection);
        page.Sections.Add(bodySection);
        page.Sections.Add(footerSection);
        report.Pages.Add(page);

        var project = new Project();
        project.Reports.Add(report);

        var contentBuilder = new ReportContentBuilder(new NoOpDataProviderRegistry());
        var exporter = new WordExporter(contentBuilder, NullLogger<WordExporter>.Instance);

        var result = await exporter.ExportAsync(project, report, ExportOptions.Default);
        Assert.True(result.IsSuccess);

        using var wordDocument = WordprocessingDocument.Open(result.Value, false);
        var mainPart = wordDocument.MainDocumentPart!;

        Assert.NotEmpty(mainPart.HeaderParts);
        Assert.NotEmpty(mainPart.FooterParts);

        var headerText = string.Join(" ", mainPart.HeaderParts.SelectMany(p => p.Header.Descendants<Text>()).Select(t => t.Text));
        Assert.Contains("Acme Corp", headerText);

        var footerText = string.Join(" ", mainPart.FooterParts.SelectMany(p => p.Footer.Descendants<Text>()).Select(t => t.Text));
        Assert.Contains("Confidential", footerText);

        var footerFields = mainPart.FooterParts.SelectMany(p => p.Footer.Descendants<SimpleField>());
        Assert.Contains(footerFields, f => f.Instruction!.Value!.Contains("PAGE"));

        var tocFields = mainPart.Document.Body!.Descendants<SimpleField>();
        Assert.Contains(tocFields, f => f.Instruction!.Value!.Contains("TOC"));

        var styles = mainPart.StyleDefinitionsPart!.Styles!.Elements<Style>();
        Assert.Contains(styles, s => s.StyleId == "Heading1");

        var sectionProperties = mainPart.Document.Body!.Elements<SectionProperties>().Single();
        Assert.NotNull(sectionProperties.GetFirstChild<PageSize>());
        Assert.NotNull(sectionProperties.GetFirstChild<PageMargin>());
    }
}
