namespace KKL.WordStudio.UI.Views;

using System.Windows.Controls;
using KKL.WordStudio.UI.ViewModels;

public partial class TablePropertiesView : UserControl
{
    public TablePropertiesView(TablePropertiesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
