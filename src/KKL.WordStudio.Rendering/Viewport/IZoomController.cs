namespace KKL.WordStudio.Rendering.Viewport;

/// <summary>Controls the design surface's zoom/pan state. No layout calculation lives here — only viewport transform.</summary>
public interface IZoomController
{
    double ZoomFactor { get; }
    void ZoomTo(double factor);
    void ZoomToFit(double contentWidth, double contentHeight, double viewportWidth, double viewportHeight);
}
