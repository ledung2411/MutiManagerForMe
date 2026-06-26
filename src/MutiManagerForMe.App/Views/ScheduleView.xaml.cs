using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MutiManagerForMe.App.Models;
using MutiManagerForMe.App.ViewModels;

namespace MutiManagerForMe.App.Views;

public partial class ScheduleView : UserControl
{
    public ScheduleView()
    {
        InitializeComponent();
    }

    private void DeleteSchedule_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ScheduleViewModel viewModel &&
            sender is FrameworkElement { DataContext: ScheduleEntry item } &&
            viewModel.DeleteCommand.CanExecute(item))
        {
            viewModel.DeleteCommand.Execute(item);
        }
    }
}
