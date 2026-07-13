using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CabinetMaster.Models;

//поля заказа
public enum OrderStatus
{
    Принят,
    В_Производстве,
    Готов
}

public partial class Order : ObservableObject
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Profit))]
    private decimal? materialCost; // Себестоимость материалов

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Profit))]
    private decimal? price; // Цена для покупателя

    [ObservableProperty] private OrderStatus status = OrderStatus.Принят;

    //конструктор заказа
    public Order(Client client, string itemName, DateTime deliveryDate, decimal? price, decimal? materialCost)
    {
        Client = client;
        ItemName = itemName;
        OrderDate = DateTime.Now;
        DeliveryDate = deliveryDate;
        Price = price;
        MaterialCost = materialCost;
    }

    public Order()
    {
    }

    public int Id { get; set; }
    public Client Client { get; set; }
    public string ItemName { get; set; } = string.Empty; // Например, "Кухня угловая"
    public DateTime OrderDate { get; } = DateTime.Now;
    public DateTime? DeliveryDate { get; set; }
    
    
    [NotMapped] public decimal? Profit// Чистая прибыль будет считаться автоматически на лету
    {
        get
        {
            if (price == null || materialCost == null) return null;
            return price.Value - materialCost.Value;
        }
        set;
    }
    public OrderStatus[] AllStatuses => (OrderStatus[])Enum.GetValues(typeof(OrderStatus));
    public ObservableCollection<OrderMaterialSpecification> MaterialsList { get; set; } = new();
}