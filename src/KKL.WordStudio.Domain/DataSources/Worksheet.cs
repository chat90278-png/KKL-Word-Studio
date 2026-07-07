namespace KKL.WordStudio.Domain.DataSources;

public sealed class Worksheet
{
    public required string Name { get; init; }
    public DataRange? SelectedRange { get; set; }
}
