namespace KKL.WordStudio.Domain.Projects;

using KKL.WordStudio.Domain.Reports;

/// <summary>
/// Project-wide defaults. Intentionally minimal today — grows as real needs
/// appear (e.g., default output folder, culture) rather than speculatively
/// front-loading fields.
/// </summary>
public sealed class ProjectSettings
{
    public PageOrientation DefaultPageOrientation { get; set; } = PageOrientation.Portrait;
    public double DefaultPageWidthMillimeters { get; set; } = 210;
    public double DefaultPageHeightMillimeters { get; set; } = 297;
}
