namespace KKL.WordStudio.Shared.Exceptions;

/// <summary>
/// Raised when a domain or application-level invariant is violated
/// (e.g., invalid report structure). Distinct from ordinary argument
/// validation, which should use standard ArgumentException subtypes.
/// </summary>
public sealed class ValidationException : WordStudioException
{
    public ValidationException(string message) : base(message) { }
}
