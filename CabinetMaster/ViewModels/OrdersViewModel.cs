using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CabinetMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CabinetMaster.ViewModels;

public partial class OrdersViewModel : ViewModelBase
{
    
    public ObservableCollection<Order> Orders { get; } = new ObservableCollection<Order>
    {
        new Order("Arseniy", "Stol", new DateTime(2026, 8, 31), 2000, 1800),
        new Order("Dmitry", "Stul", new DateTime(2026, 9, 15), 1500, 1500),
        new Order("Elena", "Shkaf", new DateTime(2026, 10, 05), 12000, 11000)
    };
    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    private async Task LoadOrdersAsync()
    {
        IsBusy =  true;
        await Task.Delay(2500);
        
        Orders.Add(new Order("Maxim", "Divan", new DateTime(2026, 11, 20), 25000, 23500));
        IsBusy =  false;
    }
    
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
        }
    }


    [ObservableProperty]
    private bool showConfirmWindow = false;
    private Order? _orderToDelete;
    [RelayCommand]
    private void DeleteOrder(Order order)
    {
        _orderToDelete = order;
        ShowConfirmWindow = true; // Меняем СВОЙСТВО (с большой буквы), чтобы UI обновился
    }

    [RelayCommand]
    private void ConfirmDelete() // Убрали параметр order, берем его из поля выше
    {
        if (_orderToDelete != null)
        {
            Orders.Remove(_orderToDelete);
            _orderToDelete = null;
        }
        ShowConfirmWindow = false; // С большой буквы
    }

    [RelayCommand]
    private void CancelDelete()
    {
        _orderToDelete = null;
        ShowConfirmWindow = false; // С большой буквы
    }
}