using Avalonia.Controls;
using Avalonia.Interactivity;
using CabinetMaster.ViewModels;

namespace CabinetMaster.Views;

public partial class OrdersView : UserControl
{
    public OrdersView()
    {
        InitializeComponent();
    }

    private void OpenOrderSpecification(object? sender, RoutedEventArgs e)
    {
        var viewModel = (OrdersViewModel)this.DataContext;
        if (viewModel?.SelectedOrder == null) return;
        if(viewModel.ShowMaterialWindow == false)
        {
            viewModel.ShowMaterialWindow = true;
        }
        else
        {
            viewModel.ShowMaterialWindow = false;
        }
    }
}