namespace KKL.WordStudio.Application.Content;

/// <summary>
/// The shared interpretation of a Report, restructured in Sprint 5 from a
/// flat node list into explicit regions. A flat list could not represent
/// "this content repeats in every page's header" vs "this is the flowing
/// body" — both real Word headers/footers and a real A4 preview need that
/// distinction. Element classification logic (what's a heading, what a
/// bound table's rows are) is unchanged from Sprint 4; only the shape of
/// the result changed.
/// </summary>
public sealed class ReportContentDocument
{
    public required IReadOnlyList<ReportContentNode> HeaderNodes { get; init; }
    public required IReadOnlyList<ReportContentNode> BodyNodes { get; init; }
    public required IReadOnlyList<ReportContentNode> FooterNodes { get; init; }

    /// <summary>Empty when Report.IncludeTableOfContents is false. Derived from Heading/AltHeading nodes already present in BodyNodes — never authored separately.</summary>
    public required IReadOnlyList<TocEntry> TableOfContents { get; init; }

    public required PageLayout PageLayout { get; init; }
}

public sealed class TocEntry
{
    public required Guid ElementId { get; init; }
    public required string Text { get; init; }
    /// <summary>1 for Heading, 2 for AltHeading — mirrors Word's Heading 1/Heading 2 TOC levels.</summary>
    public required int Level { get; init; }
}

public sealed class PageLayout
{
    public required double WidthMillimeters { get; init; }
    public required double HeightMillimeters { get; init; }
    public required double MarginTopMillimeters { get; init; }
    public required double MarginBottomMillimeters { get; init; }
    public required double MarginLeftMillimeters { get; init; }
    public required double MarginRightMillimeters { get; init; }
    public required bool ShowPageNumbers { get; init; }
}
