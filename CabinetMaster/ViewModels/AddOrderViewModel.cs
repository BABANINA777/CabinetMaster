using System;
using Avalonia.Controls.Templates;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using CabinetMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CabinetMaster.ViewModels;

public partial class AddOrderViewModel : ViewModelBase
{
    [ObservableProperty]
    private string clientName;
    [ObservableProperty]
    private string itemName;
    [ObservableProperty]
    private DateTime? deliveryDate;
    [ObservableProperty]
    private string priceString;
    [ObservableProperty]
    private string materialCostString;
    public Order? CreatedOrder { get; private set; }
    [ObservableProperty]
    private bool visibalErrorMessage = false;
    private readonly ObservableCollection<Order> _targetOrdersCollection;
    
    private readonly Action<Order?> _linkOnAddOrderClosed;

    public AddOrderViewModel(Action<Order?> linkOnAddOrderClosed)
    {
        _linkOnAddOrderClosed = linkOnAddOrderClosed;
        DeliveryDate = DateTime.Today.AddDays(7);
    }
    [RelayCommand]
    private void Save()
    {
        // проверка введеных данных
        if (string.IsNullOrWhiteSpace(ClientName) || string.IsNullOrWhiteSpace(ItemName))
        {
            VisibalErrorMessage = true;
            return;
        }
        
        // проверка на непустую дату
        if (DeliveryDate == null)
        {
            VisibalErrorMessage = true;
            return;
        }
    
        // проверка на цену
        decimal? parsedPrice = null;
        if (!string.IsNullOrWhiteSpace(PriceString))
        {
            if (!decimal.TryParse(PriceString, out decimal p) || p < 0)
            {
                VisibalErrorMessage = true;
                return;
            }
            parsedPrice = p;
        }

        decimal? parsedMaterialCost = null;
        if (!string.IsNullOrWhiteSpace(MaterialCostString))
        {
            if (!decimal.TryParse(MaterialCostString, out decimal mc) || mc < 0)
            {
                VisibalErrorMessage = true;
                return;
            }
            parsedMaterialCost = mc;
        }
        
        CreatedOrder = new Order(ClientName, ItemName, DeliveryDate.Value, parsedPrice, parsedMaterialCost);
        _linkOnAddOrderClosed(CreatedOrder);
    }

    [RelayCommand]
    private void Cancel()
    {
        _linkOnAddOrderClosed(null);
    }
}