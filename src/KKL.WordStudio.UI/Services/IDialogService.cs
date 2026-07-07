namespace KKL.WordStudio.UI.Services;

/// <summary>Abstraction over WPF dialogs so ViewModels never touch System.Windows types directly (keeps ViewModels unit-testable).</summary>
public interface IDialogService
{
    void ShowError(string message, string title = "Error");
    bool ShowConfirmation(string message, string title = "Confirm");
}
