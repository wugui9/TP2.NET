using System.Windows.Controls;
using System.Windows.Input;
using Gauniv.WpfClient.Models;
using Gauniv.WpfClient.ViewModels;

namespace Gauniv.WpfClient.Views;

public partial class GameListView : UserControl
{
    public GameListView()
    {
        InitializeComponent();
    }

    private void GameItem_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is Game game)
        {
            if (DataContext is GameListViewModel viewModel)
            {
                viewModel.ShowGameDetailsCommand.Execute(game);
            }
        }
    }

    private void CategoryComboBox_SelectionChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is GameListViewModel viewModel)
        {
            viewModel.FilterByCategoryCommand.Execute(null);
        }
    }
}
