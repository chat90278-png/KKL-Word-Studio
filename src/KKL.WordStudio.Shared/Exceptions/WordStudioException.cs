namespace KKL.WordStudio.Shared.Exceptions;

/// <summary>
/// Base type for all application-defined exceptions. Catch this (rather than
/// System.Exception) at architectural boundaries such as the UI's global
/// exception handler when the intent is "something in our domain went wrong",
/// as opposed to an unexpected framework/runtime failure.
/// </summary>
public abstract class WordStudioException : Exception
{
    protected WordStudioException(string message) : base(message) { }
    protected WordStudioException(string message, Exception innerException) : base(message, innerException) { }
}
