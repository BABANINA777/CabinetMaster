using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CabinetMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace CabinetMaster.ViewModels;

public partial class OrdersViewModel : ViewModelBase
{
    //логика кнопки обнавления данных
    private readonly CabinetMasterDbContext _context;
    private Order? _orderToDelete;


    //логика отображения окошка с добавлением заказа
    [ObservableProperty] private AddOrderViewModel? addOrderContext;

    [ObservableProperty] private string editButtonText = "Редактировать";

    [ObservableProperty] private bool isAddOrderOverlayVisible;

    [ObservableProperty] private bool isBusy;


    //логика кнопки редактировать
    [ObservableProperty] private bool isReadOnly = true;

    //логика для окошка удаления заказа
    [ObservableProperty] private bool showConfirmWindow;

    public OrdersViewModel(CabinetMasterDbContext context)
    {
        _context = context;
    }

    //список заказов
    public ObservableCollection<Order> Orders { get; } = new();

    [RelayCommand]
    private async Task LoadOrdersAsync()
    {
        IsBusy = true;
        Orders.Clear();
        var orders_db = await _context.Orders.ToListAsync();
        foreach (var zak in orders_db) Orders.Add(zak);
        IsBusy = false;
    }

    [RelayCommand]
    private void ToggleEdit()
    {
        if (EditButtonText == "Редактировать")
        {
            EditButtonText = "Готово";
            IsReadOnly = !IsReadOnly;
        }
        else
        {
            EditButtonText = "Редактировать";
            IsReadOnly = !IsReadOnly;
            _context.SaveChangesAsync();
        }
    }

    [RelayCommand]
    private void DeleteOrder(Order order)
    {
        _orderToDelete = order;
        ShowConfirmWindow = true;
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync()
    {
        if (_orderToDelete != null)
        {
            _context.Orders.Remove(_orderToDelete);
            await _context.SaveChangesAsync();

            Orders.Remove(_orderToDelete);
            _orderToDelete = null;
        }

        ShowConfirmWindow = false;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        _orderToDelete = null;
        ShowConfirmWindow = false;
    }

    public async Task AddOrderToDbAsync(Order newOrder)
    {
        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();
        Orders.Add(newOrder);
    }

    private async void OnAddOrderClosed(Order? newOrder)
    {
        IsAddOrderOverlayVisible = false;
        AddOrderContext = null;

        if (newOrder != null) await AddOrderToDbAsync(newOrder);
    }

    [RelayCommand]
    private void OpenAddOrder()
    {
        AddOrderContext = new AddOrderViewModel(OnAddOrderClosed);
        IsAddOrderOverlayVisible = true;
    }
}