namespace KKL.WordStudio.Application.Abstractions;

using KKL.WordStudio.Domain.Elements;

/// <summary>
/// Extension point for the future Property Inspector panel: lets a plugin
/// supply a custom editor UI descriptor for a given element type or
/// property, without the Property Inspector needing built-in knowledge of
/// every possible element/property combination.
/// </summary>
public interface IPropertyEditorProvider
{
    bool CanEdit(ReportElement element, string propertyName);

    /// <summary>Opaque editor descriptor key resolved by the UI layer's editor template registry.</summary>
    string GetEditorKey(ReportElement element, string propertyName);
}
