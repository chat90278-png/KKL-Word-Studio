namespace KKL.WordStudio.Rendering.Rulers;

/// <summary>Produces the tick/label positions for horizontal and vertical design-surface rulers, given the current zoom and unit of measure.</summary>
public interface IRulerService
{
    IReadOnlyList<RulerTick> GetHorizontalTicks(double viewportWidth, double zoomFactor);
    IReadOnlyList<RulerTick> GetVerticalTicks(double viewportHeight, double zoomFactor);
}

public readonly record struct RulerTick(double Position, string Label, bool IsMajor);
