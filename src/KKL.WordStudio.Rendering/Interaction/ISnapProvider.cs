namespace KKL.WordStudio.Rendering.Interaction;

using KKL.WordStudio.Shared.Geometry;

/// <summary>Computes snap targets (grid lines, element edges, alignment guides) while dragging/resizing an element.</summary>
public interface ISnapProvider
{
    PointD Snap(PointD proposedPosition, RectD movingElementBounds);
}
