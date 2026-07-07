namespace KKL.WordStudio.UI.Preview;

using KKL.WordStudio.Application.Content;
using KKL.WordStudio.Application.Preview;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;

/// <summary>
/// Maps IReportContentBuilder's shared ReportContentDocument into the
/// simple text-block-per-region DTO the Preview panel binds to. The
/// interpretation (what's a heading, what a bound table's real rows are,
/// header/body/footer separation, TOC) is genuinely correct as of Sprint
/// 5; only the *presentation* is simple (plain text, no real multi-page
/// pagination) — exactly what a future Rendering/Engine-backed
/// implementation would replace, unchanged interface, unchanged Preview
/// panel bindings.
/// </summary>
public sealed class PreviewRenderer : IReportPreviewRenderer
{
    private readonly IReportContentBuilder _contentBuilder;

    public PreviewRenderer(IReportContentBuilder contentBuilder) => _contentBuilder = contentBuilder;

    public async Task<PreviewSnapshot> RenderAsync(Project project, Report report, CancellationToken cancellationToken = default)
    {
        var document = await _contentBuilder.BuildAsync(project, report, cancellationToken);

        return new PreviewSnapshot
        {
            HeaderBlocks = BuildBlocks(document.HeaderNodes),
            BodyBlocks = BuildBlocks(document.BodyNodes),
            FooterBlocks = BuildBlocks(document.FooterNodes),
            TableOfContents = document.TableOfContents,
            PageLayout = document.PageLayout
        };
    }

    private static List<PreviewBlock> BuildBlocks(IReadOnlyList<ReportContentNode> nodes)
    {
        var blocks = new List<PreviewBlock>();

        foreach (var node in nodes)
        {
            switch (node)
            {
                case TextContentNode text:
                    blocks.Add(new PreviewBlock { ElementId = text.ElementId, Kind = text.Kind, Text = text.Text });
                    break;

                case TableContentNode table:
                    var summary = $"[Table: {table.Name}] {table.Rows.Count} row(s)"
                        + (table.DataSourceName is not null ? $" — bound to '{table.DataSourceName}'" : " — unbound")
                        + (table.FilterWasIgnored ? " (filter not yet applied)" : string.Empty);
                    blocks.Add(new PreviewBlock { ElementId = table.ElementId, Kind = ReportContentKind.Table, Text = summary });

                    if (table.ColumnHeaders.Count > 0)
                        blocks.Add(new PreviewBlock
                        {
                            ElementId = table.ElementId,
                            Kind = ReportContentKind.TableRow,
                            Text = string.Join(" | ", table.ColumnHeaders)
                        });

                    foreach (var row in table.Rows)
                        blocks.Add(new PreviewBlock
                        {
                            ElementId = table.ElementId,
                            Kind = ReportContentKind.TableRow,
                            Text = string.Join(" | ", row)
                        });
                    break;

                case ImageContentNode image:
                    blocks.Add(new PreviewBlock { ElementId = image.ElementId, Kind = ReportContentKind.Image, Text = $"[Image: {image.Name}]" });
                    break;
            }
        }

        return blocks;
    }
}
