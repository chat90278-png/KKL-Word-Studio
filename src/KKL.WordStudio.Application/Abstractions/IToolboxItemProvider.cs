namespace KKL.WordStudio.Application.Abstractions;

using KKL.WordStudio.Domain.Elements;

/// <summary>Extension point for the future Toolbox panel — lets a plugin contribute new draggable element types.</summary>
public interface IToolboxItemProvider
{
    IReadOnlyList<ToolboxItemDescriptor> GetItems();
}

public sealed class ToolboxItemDescriptor
{
    public required string DisplayName { get; init; }
    public required string Category { get; init; }
    public required Func<ReportElement> Factory { get; init; }
}
