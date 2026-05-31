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

    public AddOrderViewModel(ObservableCollection<Order> targetCollection)
    {
        _targetOrdersCollection = targetCollection;
        DeliveryDate = DateTime.Today.AddDays(7);
    }

    [RelayCommand]
    private void Save(Window window)
    {
        // 1. Валидация текстовых полей на пустоту
        if (string.IsNullOrWhiteSpace(ClientName) || string.IsNullOrWhiteSpace(ItemName))
        {
            VisibalErrorMessage = true;
            return;
        }
        if (!decimal.TryParse(PriceString, out decimal price) || 
            !decimal.TryParse(MaterialCostString, out decimal materialCost))
        {
            VisibalErrorMessage = true;
            return;
        }

        if (DeliveryDate == null)
        {
            VisibalErrorMessage = true;
            return;
        }
        CreatedOrder = new Order(ClientName, ItemName, DeliveryDate.Value, price, materialCost);
        
        
        _targetOrdersCollection.Add(CreatedOrder);
        window?.Close();

    }
        

}