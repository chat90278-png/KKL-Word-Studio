namespace KKL.WordStudio.Domain.Styling;

/// <summary>
/// Visual style for a report element. Kept as plain data (no rendering
/// logic) so it can be serialized into the .kws format and interpreted
/// independently by the Rendering engine and by each exporter.
/// </summary>
public sealed class Style
{
    public string FontFamily { get; set; } = "Segoe UI";
    public double FontSize { get; set; } = 10.0;
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }

    /// <summary>Hex color, e.g. "#FF202020". Stored as string to stay UI-framework agnostic.</summary>
    public string ForegroundColor { get; set; } = "#FF000000";
    public string? BackgroundColor { get; set; }

    public string? BorderColor { get; set; }
    public double BorderThickness { get; set; }

    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;

    /// <summary>Creates a shallow copy — used when a named/shared style is applied to an element as a starting point.</summary>
    public Style Clone() => (Style)MemberwiseClone();
}

public enum HorizontalAlignment { Left, Center, Right, Justify }
public enum VerticalAlignment { Top, Middle, Bottom }
