namespace KKL.WordStudio.Application.Styling;

using KKL.WordStudio.Domain.Styling;

/// <summary>
/// Defines what "Heading" and "Alt Heading" mean in terms of the existing
/// Style properties — no new Domain types were introduced for these (see
/// ADR 0003/0005: a Heading is just a TextElement with a particular Style).
/// This lives in Application, not UI, because more than one consumer needs
/// the exact same convention: the Report Designer (when creating a heading
/// element) and the Preview renderer (when classifying an existing element
/// for display) must agree on what counts as a heading — and a future
/// WordExporter will need the same convention to map a heading onto a
/// Word "Heading 1"-style paragraph.
/// </summary>
public static class HeadingStylePresets
{
    public const double HeadingFontSize = 18;
    public const double AltHeadingFontSize = 14;
    public const double BodyFontSize = 10;

    public static Style CreateHeadingStyle() => new() { FontSize = HeadingFontSize, Bold = true };
    public static Style CreateAltHeadingStyle() => new() { FontSize = AltHeadingFontSize, Bold = true };

    public static bool IsHeading(Style style) => style.Bold && style.FontSize >= HeadingFontSize;
    public static bool IsAltHeading(Style style) => style.Bold && style.FontSize >= AltHeadingFontSize && style.FontSize < HeadingFontSize;
}
