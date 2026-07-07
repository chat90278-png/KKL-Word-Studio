namespace KKL.WordStudio.UI;

using System.Windows;
using KKL.WordStudio.UI.ViewModels;
using KKL.WordStudio.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(
        MainViewModel viewModel,
        ProjectExplorerView projectExplorerView,
        ExcelWorkspaceView excelWorkspaceView,
        ReportDesignerView reportDesignerView,
        TablePropertiesView tablePropertiesView,
        PreviewView previewView)
    {
        InitializeComponent();
        DataContext = viewModel;

        ProjectExplorerHost.Content = projectExplorerView;
        ExcelWorkspaceHost.Content = excelWorkspaceView;
        ReportDesignerHost.Content = reportDesignerView;
        TablePropertiesHost.Content = tablePropertiesView;
        PreviewHost.Content = previewView;
    }
}
