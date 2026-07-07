namespace KKL.WordStudio.Rendering.Interaction;

using KKL.WordStudio.Shared.Geometry;

/// <summary>Anything the design surface can hit-test against (elements, selection handles, guides).</summary>
public interface IHitTestable
{
    RectD Bounds { get; }
    bool HitTest(PointD point);
}
