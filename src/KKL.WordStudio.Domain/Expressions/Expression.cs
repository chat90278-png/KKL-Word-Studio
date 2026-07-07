namespace KKL.WordStudio.Domain.Expressions;

/// <summary>
/// Represents a bound/computed value such as "=Fields.Total" or
/// "=Sum(Fields.Amount)". The Domain only stores the expression text and
/// its declared result type — actual evaluation is an execution concern
/// that belongs to the future Engine project (see ADR 0002), not to Domain.
/// </summary>
public sealed class Expression
{
    public required string Text { get; init; }
    public ExpressionResultType ResultType { get; init; } = ExpressionResultType.Text;

    public static Expression Literal(string text) => new() { Text = text };
}

public enum ExpressionResultType
{
    Text,
    Number,
    Date,
    Boolean,
    Image
}
