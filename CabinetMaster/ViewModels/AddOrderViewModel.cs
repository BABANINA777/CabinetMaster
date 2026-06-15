using System;
using System.Collections.ObjectModel;
using CabinetMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CabinetMaster.ViewModels;

public partial class AddOrderViewModel : ViewModelBase
{
    private readonly Action<Order?> _linkOnAddOrderClosed;
    private readonly ObservableCollection<Order> _targetOrdersCollection;

    [ObservableProperty] private string clientName;

    [ObservableProperty] private DateTime? deliveryDate;

    [ObservableProperty] private string itemName;

    [ObservableProperty] private string materialCostString;

    [ObservableProperty] private string priceString;

    [ObservableProperty] private bool visibalErrorMessage;

    public AddOrderViewModel(Action<Order?> linkOnAddOrderClosed)
    {
        _linkOnAddOrderClosed = linkOnAddOrderClosed;
        DeliveryDate = DateTime.Today.AddDays(7);
    }

    public Order? CreatedOrder { get; private set; }

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
            if (!decimal.TryParse(PriceString, out var p) || p < 0)
            {
                VisibalErrorMessage = true;
                return;
            }

            parsedPrice = p;
        }

        decimal? parsedMaterialCost = null;
        if (!string.IsNullOrWhiteSpace(MaterialCostString))
        {
            if (!decimal.TryParse(MaterialCostString, out var mc) || mc < 0)
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