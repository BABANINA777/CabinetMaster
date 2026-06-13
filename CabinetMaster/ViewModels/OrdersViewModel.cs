using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CabinetMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace CabinetMaster.ViewModels;

public partial class OrdersViewModel : ViewModelBase
{
    //список заказов
    public ObservableCollection<Order> Orders { get; } = new ObservableCollection<Order>();
    
    //логика кнопки обнавления данных
    private readonly CabinetMasterDbContext _context;
    public OrdersViewModel(CabinetMasterDbContext context)
    {
        _context = context;
    }
    
    [ObservableProperty]
    private bool isBusy;
    [RelayCommand]
    private async Task LoadOrdersAsync()
    {
        IsBusy =  true;
        Orders.Clear();
        var orders_db = await _context.Orders.ToListAsync();
        foreach (var zak in orders_db)
        {
            Orders.Add(zak);
        }
        IsBusy =  false;
    }
    
    
    //логика кнопки редактировать
    [ObservableProperty]
    private bool isReadOnly = true;

    [ObservableProperty] private string editButtonText = "Редактировать";
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

    //логика для окошка удаления заказа
    [ObservableProperty]
    private bool showConfirmWindow = false;
    private Order? _orderToDelete;
    
    [RelayCommand] private void DeleteOrder(Order order)
    {
        _orderToDelete = order;
        ShowConfirmWindow = true;
    }
    
    [RelayCommand] private async Task ConfirmDeleteAsync()
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
    
    [RelayCommand] private void CancelDelete()
    {
        _orderToDelete = null;
        ShowConfirmWindow = false;
    }

    
    //логика отображения окошка с добавлением заказа
    [ObservableProperty]
    private AddOrderViewModel? addOrderContext;

    [ObservableProperty]
    private bool isAddOrderOverlayVisible;
    
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

        if (newOrder != null)
        {
            await AddOrderToDbAsync(newOrder);
        }
    }
    [RelayCommand]
    private void OpenAddOrder()
    {
        AddOrderContext = new AddOrderViewModel(OnAddOrderClosed);
        IsAddOrderOverlayVisible = true;
    }
}