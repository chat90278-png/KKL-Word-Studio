namespace KKL.WordStudio.Shared.Guards;

/// <summary>
/// Small argument-validation helpers to keep constructors and methods free
/// of repetitive null/empty checks. Intentionally minimal — this is not a
/// full guard-clause library, just the handful of checks used everywhere.
/// </summary>
public static class Guard
{
    public static T AgainstNull<T>(T? value, string paramName) where T : class
        => value ?? throw new ArgumentNullException(paramName);

    public static string AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        return value;
    }

    public static int AgainstNegative(int value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, value, "Value cannot be negative.");
        return value;
    }
}
