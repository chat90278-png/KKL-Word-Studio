namespace KKL.WordStudio.Application.Content;

using KKL.WordStudio.Application.Abstractions;
using KKL.WordStudio.Application.Styling;
using KKL.WordStudio.Domain.DataBinding;
using KKL.WordStudio.Domain.Elements;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;

/// <summary>
/// Default IReportContentBuilder. Splits a Report's sections into three
/// regions by Section.Kind: PageHeader/PageFooter sections become the
/// repeating Header/Footer regions; everything else (Body, ReportHeader,
/// ReportFooter, GroupHeader, GroupFooter) flows into the Body region in
/// page/section order — a deliberate simplification for this "foundation"
/// sprint (see ADR 0007) rather than modeling per-group repetition, which
/// is real pagination/execution work for the future Engine (ADR 0002).
///
/// PageLayout is taken from the Report's first Page — every Section/
/// Element after that shares one page template, consistent with how the
/// Report Designer only creates a single Page today.
/// </summary>
public sealed class ReportContentBuilder : IReportContentBuilder
{
    private readonly IDataProviderRegistry _dataProviderRegistry;

    public ReportContentBuilder(IDataProviderRegistry dataProviderRegistry) => _dataProviderRegistry = dataProviderRegistry;

    public async Task<ReportContentDocument> BuildAsync(Project project, Report report, CancellationToken cancellationToken = default)
    {
        var headerNodes = new List<ReportContentNode>();
        var bodyNodes = new List<ReportContentNode>();
        var footerNodes = new List<ReportContentNode>();

        foreach (var page in report.Pages)
        {
            foreach (var section in page.Sections)
            {
                var target = section.Kind switch
                {
                    SectionKind.PageHeader => headerNodes,
                    SectionKind.PageFooter => footerNodes,
                    _ => bodyNodes
                };
                await BuildFromContainerAsync(project, section.Root, target, cancellationToken);
            }
        }

        var tableOfContents = report.IncludeTableOfContents
            ? BuildTableOfContents(bodyNodes)
            : Array.Empty<TocEntry>();

        var firstPage = report.Pages.FirstOrDefault();
        var pageLayout = new PageLayout
        {
            WidthMillimeters = firstPage?.WidthMillimeters ?? 210,
            HeightMillimeters = firstPage?.HeightMillimeters ?? 297,
            MarginTopMillimeters = firstPage?.MarginsMillimeters.Top ?? 20,
            MarginBottomMillimeters = firstPage?.MarginsMillimeters.Bottom ?? 20,
            MarginLeftMillimeters = firstPage?.MarginsMillimeters.Left ?? 20,
            MarginRightMillimeters = firstPage?.MarginsMillimeters.Right ?? 20,
            ShowPageNumbers = firstPage?.ShowPageNumbers ?? true
        };

        return new ReportContentDocument
        {
            HeaderNodes = headerNodes,
            BodyNodes = bodyNodes,
            FooterNodes = footerNodes,
            TableOfContents = tableOfContents,
            PageLayout = pageLayout
        };
    }

    private static IReadOnlyList<TocEntry> BuildTableOfContents(IReadOnlyList<ReportContentNode> bodyNodes) =>
        bodyNodes
            .OfType<TextContentNode>()
            .Where(t => t.Kind is ReportContentKind.Heading or ReportContentKind.AltHeading)
            .Select(t => new TocEntry
            {
                ElementId = t.ElementId,
                Text = t.Text,
                Level = t.Kind == ReportContentKind.Heading ? 1 : 2
            })
            .ToList();

    private async Task BuildFromContainerAsync(Project project, Container container, List<ReportContentNode> nodes, CancellationToken cancellationToken)
    {
        foreach (var child in container.Children)
        {
            switch (child)
            {
                case Container nested:
                    await BuildFromContainerAsync(project, nested, nodes, cancellationToken);
                    break;

                case TextElement text:
                    nodes.Add(BuildTextNode(text));
                    break;

                case ImageElement image:
                    nodes.Add(new ImageContentNode { ElementId = image.Id, Kind = ReportContentKind.Image, Name = image.Name });
                    break;

                case TableElement table:
                    nodes.Add(await BuildTableNodeAsync(project, table, cancellationToken));
                    break;
            }
        }
    }

    private static TextContentNode BuildTextNode(TextElement text)
    {
        var kind = HeadingStylePresets.IsHeading(text.Style) ? ReportContentKind.Heading
            : HeadingStylePresets.IsAltHeading(text.Style) ? ReportContentKind.AltHeading
            : ReportContentKind.Paragraph;

        return new TextContentNode
        {
            ElementId = text.Id,
            Kind = kind,
            Text = text.Content.Text,
            Bold = text.Style.Bold,
            FontSize = text.Style.FontSize
        };
    }

    private async Task<TableContentNode> BuildTableNodeAsync(Project project, TableElement table, CancellationToken cancellationToken)
    {
        if (table.Binding is not null)
        {
            var dataSource = project.DataSources.FirstOrDefault(ds => ds.Name == table.Binding.DataSourceName);
            if (dataSource is not null)
            {
                var provider = _dataProviderRegistry.Resolve(dataSource.ProviderKey);
                var rowsResult = await provider.GetRowsAsync(dataSource, cancellationToken);

                if (rowsResult.IsSuccess)
                {
                    var fieldNames = dataSource.Fields.Select(f => f.Name).ToList();
                    IEnumerable<IReadOnlyDictionary<string, object?>> rows = rowsResult.Value;

                    if (table.Binding.SortFields.Count > 0)
                        rows = ApplySort(rows, table.Binding.SortFields);

                    var renderedRows = rows
                        .Select(row => (IReadOnlyList<string>)fieldNames
                            .Select(field => row.TryGetValue(field, out var value) ? value?.ToString() ?? string.Empty : string.Empty)
                            .ToList())
                        .ToList();

                    return new TableContentNode
                    {
                        ElementId = table.Id,
                        Kind = ReportContentKind.Table,
                        Name = table.Name,
                        ColumnHeaders = fieldNames,
                        Rows = renderedRows,
                        DataSourceName = dataSource.Name,
                        // Filter is intentionally not applied yet — it requires an expression
                        // evaluator, which is future Engine work (ADR 0002/0004/0006). Surfacing
                        // this flag rather than silently ignoring the filter.
                        FilterWasIgnored = table.Binding.Filter is not null
                    };
                }
            }
        }

        // Unbound (static) table: use the manually-authored Columns/Rows.
        // Note: the Sprint 3 Report Designer's "Add Table" does not yet populate
        // per-cell content, so static tables currently render header-only until a
        // cell-editing UI exists — a known, deliberately out-of-scope gap.
        var headers = table.Columns.Select(c => c.Header).ToList();
        var staticRows = table.Rows
            .Where(r => r.Kind == TableRowKind.Detail)
            .Select(r => (IReadOnlyList<string>)r.Cells.Select(ExtractCellText).ToList())
            .ToList();

        return new TableContentNode
        {
            ElementId = table.Id,
            Kind = ReportContentKind.Table,
            Name = table.Name,
            ColumnHeaders = headers,
            Rows = staticRows,
            DataSourceName = null,
            FilterWasIgnored = false
        };
    }

    private static string ExtractCellText(Container cell) =>
        cell.Children.OfType<TextElement>().FirstOrDefault()?.Content.Text ?? string.Empty;

    private static IEnumerable<IReadOnlyDictionary<string, object?>> ApplySort(
        IEnumerable<IReadOnlyDictionary<string, object?>> rows, IReadOnlyList<SortField> sortFields)
    {
        IOrderedEnumerable<IReadOnlyDictionary<string, object?>>? ordered = null;

        foreach (var sortField in sortFields)
        {
            object? KeySelector(IReadOnlyDictionary<string, object?> row) =>
                row.TryGetValue(sortField.FieldName, out var value) ? value : null;

            ordered = ordered is null
                ? sortField.Direction == SortDirection.Ascending ? rows.OrderBy(KeySelector) : rows.OrderByDescending(KeySelector)
                : sortField.Direction == SortDirection.Ascending ? ordered.ThenBy(KeySelector) : ordered.ThenByDescending(KeySelector);
        }

        return ordered ?? rows;
    }
}
