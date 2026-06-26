using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MutiManagerForMe.App.Models;
using MutiManagerForMe.App.ViewModels;

namespace MutiManagerForMe.App.Views;

public partial class TasksView : UserControl
{
    public TasksView()
    {
        InitializeComponent();
    }

    private void ToggleTask_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is TasksViewModel viewModel &&
            sender is FrameworkElement { DataContext: TaskItem item } &&
            viewModel.ToggleCompletedCommand.CanExecute(item))
        {
            viewModel.ToggleCompletedCommand.Execute(item);
        }
    }

    private void DeleteTask_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is TasksViewModel viewModel &&
            sender is FrameworkElement { DataContext: TaskItem item } &&
            viewModel.DeleteCommand.CanExecute(item))
        {
            viewModel.DeleteCommand.Execute(item);
        }
    }
}
