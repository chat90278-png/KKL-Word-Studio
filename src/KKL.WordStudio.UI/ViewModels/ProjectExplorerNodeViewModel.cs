namespace KKL.WordStudio.UI.ViewModels;

using System.Collections.ObjectModel;

/// <summary>Generic tree node for the Project Explorer — deliberately untyped (just Name + Children + an optional click action) so new branches (Templates, Assets, ...) can be added later without a new node type per category.</summary>
public sealed class ProjectExplorerNodeViewModel
{
    public required string Name { get; init; }
    public ObservableCollection<ProjectExplorerNodeViewModel> Children { get; } = new();
    public Action? OnSelected { get; init; }
}
