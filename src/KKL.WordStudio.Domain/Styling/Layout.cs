namespace KKL.WordStudio.Domain.Styling;

using KKL.WordStudio.Shared.Geometry;

/// <summary>Positioning and sizing information for a report element, independent of any rendering surface.</summary>
public sealed class Layout
{
    public RectD Bounds { get; set; }
    public Thickness Margin { get; set; } = Thickness.Zero;
    public Thickness Padding { get; set; } = Thickness.Zero;
    public bool IsVisible { get; set; } = true;

    /// <summary>Docking/anchoring behavior when the containing Section resizes.</summary>
    public AnchorMode Anchor { get; set; } = AnchorMode.TopLeft;
}

public readonly record struct Thickness(double Left, double Top, double Right, double Bottom)
{
    public static readonly Thickness Zero = new(0, 0, 0, 0);
    public static Thickness Uniform(double value) => new(value, value, value, value);
}

public enum AnchorMode
{
    TopLeft,
    Top,
    TopRight,
    Left,
    Center,
    Right,
    BottomLeft,
    Bottom,
    BottomRight,
    Stretch
}
