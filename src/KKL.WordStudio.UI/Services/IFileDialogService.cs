namespace KKL.WordStudio.UI.Services;

/// <summary>Abstraction over WPF's file dialogs so ViewModels never reference Microsoft.Win32 types directly (keeps ViewModels unit-testable).</summary>
public interface IFileDialogService
{
    string? OpenExcelFile();
    string? SaveProjectFile(string suggestedFileName);
    string? SaveWordFile(string suggestedFileName);
}
