namespace KKL.WordStudio.Domain.Elements;

using KKL.WordStudio.Domain.Visitors;

public sealed class ImageElement : ReportElement
{
    /// <summary>Reference into the .kws package's /resources/images entries — never a raw file-system path.</summary>
    public string ResourceKey { get; set; } = string.Empty;
    public ImageStretchMode Stretch { get; set; } = ImageStretchMode.Uniform;

    public override void Accept(IReportElementVisitor visitor) => visitor.Visit(this);
}

public enum ImageStretchMode { None, Fill, Uniform, UniformToFill }
