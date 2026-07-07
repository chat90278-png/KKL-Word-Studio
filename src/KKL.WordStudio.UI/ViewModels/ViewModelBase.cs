namespace KKL.WordStudio.UI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// Common base for all ViewModels. Currently just aliases
/// CommunityToolkit's ObservableObject, but having our own base type means
/// we can add cross-cutting behavior later (e.g., IsBusy, error surface)
/// without touching every derived ViewModel's inheritance.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
}
