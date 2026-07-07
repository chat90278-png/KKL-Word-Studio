namespace KKL.WordStudio.UI.Views;

using System.Windows.Controls;
using KKL.WordStudio.UI.ViewModels;

public partial class PreviewView : UserControl
{
    public PreviewView(PreviewViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
