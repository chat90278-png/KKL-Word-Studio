namespace KKL.WordStudio.UI.Services;

using Microsoft.Win32;

public sealed class FileDialogService : IFileDialogService
{
    public string? OpenExcelFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel Workbooks (*.xlsx)|*.xlsx",
            Title = "Open Excel File"
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SaveProjectFile(string suggestedFileName)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "KKL Word Studio Project (*.kws)|*.kws",
            FileName = suggestedFileName,
            Title = "Save Project"
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SaveWordFile(string suggestedFileName)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Word Document (*.docx)|*.docx",
            FileName = suggestedFileName,
            Title = "Export to Word"
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
