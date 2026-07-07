namespace KKL.WordStudio.Shared.Extensions;

public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    /// <summary>Returns a short, stable id-friendly slug (used for element names, plugin keys, etc.).</summary>
    public static string ToSlug(this string value)
        => new string(value.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());
}
