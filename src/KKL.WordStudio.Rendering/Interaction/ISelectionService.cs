namespace KKL.WordStudio.Rendering.Interaction;

using KKL.WordStudio.Domain.Elements;

/// <summary>Tracks which report elements are currently selected on the design surface. Purely a UI/interaction concern — carries no layout or execution logic.</summary>
public interface ISelectionService
{
    IReadOnlyCollection<ReportElement> SelectedElements { get; }

    void Select(ReportElement element, bool addToSelection = false);
    void Clear();

    event EventHandler? SelectionChanged;
}
