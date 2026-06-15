using System;
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
    public Order(string clientName, string itemName, DateTime deliveryDate, decimal? price, decimal? materialCost)
    {
        ClientName = clientName;
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
    public string ClientName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty; // Например, "Кухня угловая"
    public DateTime OrderDate { get; } = DateTime.Now;

    public DateTime? DeliveryDate { get; set; }

    // Чистая прибыль будет считаться автоматически на лету
    [NotMapped]
    public decimal? Profit
    {
        get
        {
            if (price == null || materialCost == null) return null;
            return price.Value - materialCost.Value;
        }
        set;
    }

    public OrderStatus[] AllStatuses => (OrderStatus[])Enum.GetValues(typeof(OrderStatus));
}