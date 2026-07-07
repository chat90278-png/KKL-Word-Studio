namespace KKL.WordStudio.Domain.Reports;

using KKL.WordStudio.Domain.Styling;

/// <summary>
/// Describes page geometry and the ordered set of Sections placed on it.
/// A Report can define multiple Page templates (e.g., a cover page vs. the
/// main content pages) — see "Multi-page Designer" in the roadmap.
/// </summary>
public sealed class Page
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Page1";

    public double WidthMillimeters { get; set; } = 210;   // A4 default
    public double HeightMillimeters { get; set; } = 297;
    public Thickness MarginsMillimeters { get; set; } = Thickness.Uniform(20);
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;

    /// <summary>Whether the footer displays a page number field. Real page numbering (PAGE field in Word; approximate "Page 1" in Preview) — see ADR 0007.</summary>
    public bool ShowPageNumbers { get; set; } = true;

    public List<Section> Sections { get; } = new();
}

public enum PageOrientation { Portrait, Landscape }
