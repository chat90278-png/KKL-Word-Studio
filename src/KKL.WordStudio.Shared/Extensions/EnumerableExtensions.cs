namespace KKL.WordStudio.Shared.Extensions;

public static class EnumerableExtensions
{
    /// <summary>Null-safe empty-check without allocating a materialized list.</summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) => source is null || !source.Any();

    /// <summary>Depth-first flattening — used for walking the report element tree in tooling/tests.</summary>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector)
    {
        foreach (var item in source)
        {
            yield return item;
            foreach (var child in Flatten(childrenSelector(item), childrenSelector))
                yield return child;
        }
    }
}
