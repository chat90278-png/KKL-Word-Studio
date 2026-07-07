namespace KKL.WordStudio.UI.ViewModels;

public sealed class TocEntryViewModel
{
    public required string Text { get; init; }
    public required int Level { get; init; }
    public double Indent => (Level - 1) * 16;
}
