namespace KKL.WordStudio.Application.Content;

/// <summary>
/// The single, shared classification of a report element's content type.
/// Both the Preview renderer and WordExporter ask the exact same question
/// (via ReportContentBuilder) and get the exact same answer for the same
/// element — this is what makes "Preview and Export don't use separate
/// logic" true in code, not just in intent.
/// </summary>
public enum ReportContentKind
{
    Heading,
    AltHeading,
    Paragraph,
    Table,
    TableRow,
    Image,
    Other
}
