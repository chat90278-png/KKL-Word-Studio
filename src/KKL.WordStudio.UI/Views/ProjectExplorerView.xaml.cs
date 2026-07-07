namespace KKL.WordStudio.UI.Views;

using System.Windows;
using System.Windows.Controls;
using KKL.WordStudio.UI.ViewModels;

public partial class ProjectExplorerView : UserControl
{
    public ProjectExplorerView(ProjectExplorerViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is ProjectExplorerNodeViewModel node)
            node.OnSelected?.Invoke();
    }
}
