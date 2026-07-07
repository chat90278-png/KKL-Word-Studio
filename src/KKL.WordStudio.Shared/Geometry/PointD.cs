namespace KKL.WordStudio.Shared.Geometry;

/// <summary>
/// Framework-independent point. Deliberately not System.Windows.Point —
/// Domain and Rendering must stay usable outside WPF (unit tests, future
/// alternate front-ends), so geometry primitives live here instead.
/// </summary>
public readonly record struct PointD(double X, double Y);
