using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using CabinetMaster.Models;
using CabinetMaster.ViewModels;
using CabinetMaster.Views; 

namespace CabinetMaster.Views;

public partial class OrdersView : UserControl
{
    public OrdersView()
    {
        InitializeComponent();
    }
    
    private async void AddOrder_Click(object? sender, RoutedEventArgs e)
    {
        var mainVm = (OrdersViewModel)this.DataContext;
        var dialog = new AddOrderWindow();
        var context = new AddOrderViewModel(mainVm.Orders);
        
        dialog.DataContext = context;
        await dialog.ShowDialog((Window)TopLevel.GetTopLevel(this));
        
    }
}