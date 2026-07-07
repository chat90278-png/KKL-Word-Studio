namespace KKL.WordStudio.UI.ViewModels;

using KKL.WordStudio.Application.Content;

public sealed class PreviewBlockViewModel
{
    public required Guid ElementId { get; init; }
    public required ReportContentKind Kind { get; init; }
    public required string Text { get; init; }
}
