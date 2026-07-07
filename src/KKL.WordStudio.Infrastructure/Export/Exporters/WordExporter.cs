namespace KKL.WordStudio.Infrastructure.Export.Exporters;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using KKL.WordStudio.Application.Abstractions;
using KKL.WordStudio.Application.Content;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;
using KKL.WordStudio.Shared.Results;
using Microsoft.Extensions.Logging;

/// <summary>
/// Translates the abstract Report model into a .docx stream using the
/// OpenXML SDK. Consumes exactly the same IReportContentBuilder output
/// (header/body/footer regions, TOC, page layout) the Preview panel
/// renders — the two can never disagree about document structure
/// (Sprint 4's core requirement, extended in Sprint 5 to real page
/// layout/header/footer/page numbers/TOC).
///
/// Headings use real named paragraph styles (Heading1/Heading2, with
/// outlineLvl set) rather than Sprint 4's direct-formatting-only
/// approach — Word's native TOC field only collects paragraphs that carry
/// an outline level, so a real TOC forced this through. Direct run
/// formatting is still applied alongside the style as a visual safety net.
/// </summary>
public sealed class WordExporter : IReportExporter
{
    private const double TwipsPerMillimeter = 1440.0 / 25.4;

    private readonly IReportContentBuilder _contentBuilder;
    private readonly ILogger<WordExporter> _logger;

    public WordExporter(IReportContentBuilder contentBuilder, ILogger<WordExporter> logger)
    {
        _contentBuilder = contentBuilder;
        _logger = logger;
    }

    public string FormatKey => "docx";
    public string DisplayName => "Word Document (.docx)";

    public async Task<Result<Stream>> ExportAsync(Project project, Report report, ExportOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _contentBuilder.BuildAsync(project, report, cancellationToken);

            var stream = new MemoryStream();
            using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, autoSave: false))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                AddStyleDefinitions(mainPart);

                if (report.IncludeTableOfContents && document.TableOfContents.Count > 0)
                    body.AppendChild(BuildTocParagraph());

                foreach (var node in document.BodyNodes)
                    AppendNode(body, node);

                var sectionProperties = new SectionProperties();
                AppendHeaderFooterReferences(mainPart, sectionProperties, document);
                AppendPageLayout(sectionProperties, document.PageLayout);
                body.AppendChild(sectionProperties);

                mainPart.Document.Save();
            }

            stream.Position = 0;
            return Result.Success<Stream>(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export report {ReportId} to Word", report.Id);
            return Result.Failure<Stream>($"Word export failed: {ex.Message}");
        }
    }

    // ---- Styles (Heading1/Heading2 with outline levels, so Word's native TOC field can find them) ----

    private static void AddStyleDefinitions(MainDocumentPart mainPart)
    {
        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        var styles = new Styles();

        styles.Append(new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Normal",
            Default = true,
            StyleName = new StyleName { Val = "Normal" }
        });

        styles.Append(BuildHeadingStyle("Heading1", "heading 1", outlineLevel: 0, fontSizeHalfPoints: "36"));
        styles.Append(BuildHeadingStyle("Heading2", "heading 2", outlineLevel: 1, fontSizeHalfPoints: "28"));

        stylesPart.Styles = styles;
    }

    private static Style BuildHeadingStyle(string styleId, string name, int outlineLevel, string fontSizeHalfPoints) => new()
    {
        Type = StyleValues.Paragraph,
        StyleId = styleId,
        StyleName = new StyleName { Val = name },
        BasedOn = new BasedOn { Val = "Normal" },
        StyleParagraphProperties = new StyleParagraphProperties(new OutlineLevel { Val = outlineLevel }),
        StyleRunProperties = new StyleRunProperties(new Bold(), new FontSize { Val = fontSizeHalfPoints })
    };

    // ---- Header / Footer parts ----

    private static void AppendHeaderFooterReferences(MainDocumentPart mainPart, SectionProperties sectionProperties, ReportContentDocument document)
    {
        if (document.HeaderNodes.Count > 0)
        {
            var headerPart = mainPart.AddNewPart<HeaderPart>();
            var header = new Header();
            foreach (var node in document.HeaderNodes)
                AppendNode(header, node);
            headerPart.Header = header;

            sectionProperties.Append(new HeaderReference { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(headerPart) });
        }

        if (document.FooterNodes.Count > 0 || document.PageLayout.ShowPageNumbers)
        {
            var footerPart = mainPart.AddNewPart<FooterPart>();
            var footer = new Footer();
            foreach (var node in document.FooterNodes)
                AppendNode(footer, node);

            if (document.PageLayout.ShowPageNumbers)
                footer.AppendChild(BuildPageNumberParagraph());

            footerPart.Footer = footer;
            sectionProperties.Append(new FooterReference { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(footerPart) });
        }
    }

    private static Paragraph BuildPageNumberParagraph()
    {
        var field = new SimpleField(new Run(new Text("1"))) { Instruction = " PAGE " };
        var paragraph = new Paragraph(new ParagraphProperties(new Justification { Val = JustificationValues.Right }));
        paragraph.AppendChild(field);
        return paragraph;
    }

    private static Paragraph BuildTocParagraph()
    {
        var field = new SimpleField(new Run(new Text("Right-click and choose \"Update Field\" to generate the table of contents.")))
        {
            Instruction = @" TOC \o ""1-2"" \h \z \u "
        };
        var paragraph = new Paragraph();
        paragraph.AppendChild(field);
        return paragraph;
    }

    // ---- Page size/margins ----

    private static void AppendPageLayout(SectionProperties sectionProperties, PageLayout layout)
    {
        var widthTwips = (uint)Math.Round(layout.WidthMillimeters * TwipsPerMillimeter);
        var heightTwips = (uint)Math.Round(layout.HeightMillimeters * TwipsPerMillimeter);

        sectionProperties.Append(new PageSize { Width = widthTwips, Height = heightTwips });
        sectionProperties.Append(new PageMargin
        {
            Top = (int)Math.Round(layout.MarginTopMillimeters * TwipsPerMillimeter),
            Bottom = (int)Math.Round(layout.MarginBottomMillimeters * TwipsPerMillimeter),
            Left = (uint)Math.Round(layout.MarginLeftMillimeters * TwipsPerMillimeter),
            Right = (uint)Math.Round(layout.MarginRightMillimeters * TwipsPerMillimeter),
            Header = 0,
            Footer = 0
        });
    }

    // ---- Content emission (shared shape for Body, Header, Footer containers) ----

    private static void AppendNode(OpenXmlCompositeElement container, ReportContentNode node)
    {
        switch (node)
        {
            case TextContentNode text:
                container.AppendChild(BuildParagraph(text));
                break;
            case TableContentNode table:
                if (table.Rows.Count == 0 && table.ColumnHeaders.Count == 0)
                    break; // nothing configured yet — skip rather than emit an empty shell
                container.AppendChild(BuildTable(table));
                break;
            case ImageContentNode image:
                // Real image embedding needs the Asset/resource catalog (deliberately
                // deferred — ADR 0004). A text placeholder keeps the document's
                // structure correct without silently dropping the element.
                container.AppendChild(BuildPlainParagraph($"[Image: {image.Name}]"));
                break;
        }
    }

    private static Paragraph BuildParagraph(TextContentNode text)
    {
        var paragraph = new Paragraph();

        if (text.Kind is ReportContentKind.Heading or ReportContentKind.AltHeading)
        {
            var styleId = text.Kind == ReportContentKind.Heading ? "Heading1" : "Heading2";
            paragraph.ParagraphProperties = new ParagraphProperties(new ParagraphStyleId { Val = styleId });
        }

        var fontSizeHalfPoints = (int)(text.FontSize * 2);
        var bold = text.Kind is ReportContentKind.Heading or ReportContentKind.AltHeading || text.Bold;

        var runProperties = new RunProperties(new FontSize { Val = fontSizeHalfPoints.ToString() });
        if (bold) runProperties.Append(new Bold());

        paragraph.AppendChild(new Run(runProperties, new Text(text.Text) { Space = SpaceProcessingModeValues.Preserve }));
        return paragraph;
    }

    private static Paragraph BuildPlainParagraph(string text) => new(new Run(new Text(text)));

    private static Table BuildTable(TableContentNode tableNode)
    {
        var table = new Table();

        var tableProperties = new TableProperties(
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4 },
                new BottomBorder { Val = BorderValues.Single, Size = 4 },
                new LeftBorder { Val = BorderValues.Single, Size = 4 },
                new RightBorder { Val = BorderValues.Single, Size = 4 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }));
        table.AppendChild(tableProperties);

        if (tableNode.ColumnHeaders.Count > 0)
            table.AppendChild(BuildRow(tableNode.ColumnHeaders, bold: true));

        foreach (var row in tableNode.Rows)
            table.AppendChild(BuildRow(row, bold: false));

        return table;
    }

    private static TableRow BuildRow(IReadOnlyList<string> cellValues, bool bold)
    {
        var row = new TableRow();
        foreach (var value in cellValues)
        {
            var runProperties = bold ? new RunProperties(new Bold()) : new RunProperties();
            var cell = new TableCell(new Paragraph(new Run(runProperties, new Text(value) { Space = SpaceProcessingModeValues.Preserve })));
            row.AppendChild(cell);
        }
        return row;
    }
}
