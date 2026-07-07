namespace KKL.WordStudio.Shared.Geometry;

public readonly record struct RectD(double X, double Y, double Width, double Height)
{
    public double Left => X;
    public double Top => Y;
    public double Right => X + Width;
    public double Bottom => Y + Height;

    public bool Contains(PointD point) =>
        point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;

    public bool IntersectsWith(RectD other) =>
        Left < other.Right && Right > other.Left && Top < other.Bottom && Bottom > other.Top;
}
