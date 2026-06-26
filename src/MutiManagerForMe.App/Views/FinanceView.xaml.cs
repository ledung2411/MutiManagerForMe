using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MutiManagerForMe.App.Models;
using MutiManagerForMe.App.ViewModels;

namespace MutiManagerForMe.App.Views;

public partial class FinanceView : UserControl
{
    public FinanceView()
    {
        InitializeComponent();
    }

    private void DeleteTransaction_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is FinanceViewModel viewModel &&
            sender is FrameworkElement { DataContext: FinanceTransaction item } &&
            viewModel.DeleteTransactionCommand.CanExecute(item))
        {
            viewModel.DeleteTransactionCommand.Execute(item);
        }
    }
}
