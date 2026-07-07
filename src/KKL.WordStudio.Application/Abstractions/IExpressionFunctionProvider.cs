namespace KKL.WordStudio.Application.Abstractions;

/// <summary>
/// Extension point for the future formula engine's function library
/// (Sum, Avg, IIf, custom business functions, ...). Registered functions
/// are consumed by the Engine project once it exists; Application only
/// owns the registration contract.
/// </summary>
public interface IExpressionFunctionProvider
{
    IReadOnlyList<string> GetFunctionNames();
}
