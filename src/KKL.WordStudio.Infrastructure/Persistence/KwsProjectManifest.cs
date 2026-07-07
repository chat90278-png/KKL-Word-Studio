namespace KKL.WordStudio.Infrastructure.Persistence;

/// <summary>Serialized as manifest.json inside the .kws package. Kept separate from the report content for fast metadata reads (e.g., recent-files list) without parsing the whole report.</summary>
public sealed class KwsProjectManifest
{
    public required string FormatVersion { get; init; }
    public required string ProductVersion { get; init; }
    public DateTimeOffset CreatedUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedUtc { get; set; } = DateTimeOffset.UtcNow;
}
