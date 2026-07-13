using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CabinetMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace CabinetMaster.ViewModels;

public partial class AddOrderViewModel : ViewModelBase
{
    private readonly Action<Order?> _linkOnAddOrderClosed;
    private readonly ObservableCollection<Order> _targetOrdersCollection;
    private readonly CabinetMasterDbContext _context;

    [ObservableProperty] private Client? client;

    [ObservableProperty] private DateTime? deliveryDate;

    [ObservableProperty] private string itemName;

    [ObservableProperty] private string materialCostString;

    [ObservableProperty] private string priceString;

    [ObservableProperty] private bool visibalErrorMessage;

    public IEnumerable<Client> Clients
    {
        get
        {
            return GetClientNamesAsync().GetAwaiter().GetResult();
        }
    }
    public async Task<List<Client>> GetClientNamesAsync()
    {
        return await _context.Clients.ToListAsync();
    }
    public AddOrderViewModel(CabinetMasterDbContext context, Action<Order?> linkOnAddOrderClosed)
    {
        _context = context;
        _linkOnAddOrderClosed = linkOnAddOrderClosed;
        DeliveryDate = DateTime.Today.AddDays(7);
    }

    public Order? CreatedOrder { get; private set; }

    [RelayCommand]
    private void Save()
    {
        //проверка выбрали ли клиента
        if (Client == null)
        {
            VisibalErrorMessage = true;
            return;
        }
        // проверка введеных данных
        if (string.IsNullOrWhiteSpace(ItemName))
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

        CreatedOrder = new Order(Client, ItemName, DeliveryDate.Value, parsedPrice, parsedMaterialCost);
        _linkOnAddOrderClosed(CreatedOrder);
    }

    [RelayCommand]
    private void Cancel()
    {
        _linkOnAddOrderClosed(null);
    }
}