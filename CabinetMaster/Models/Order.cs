using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CabinetMaster.Models;

public partial class Order : ObservableObject
{
    public enum OrderStatus
    {
        Принят,
        В_Производстве,
        Готов
    }
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty; // Например, "Кухня угловая"
    public DateTime OrderDate { get; } = DateTime.Now;
    public DateTime DeliveryDate { get; set; }
    [ObservableProperty][NotifyPropertyChangedFor(nameof(Profit))] private decimal price;          // Цена для покупателя
    [ObservableProperty][NotifyPropertyChangedFor(nameof(Profit))] private decimal materialCost;    // Себестоимость материалов
    // Чистая прибыль будет считаться автоматически на лету
    public decimal Profit => Price - MaterialCost; 
    public OrderStatus[] AllStatuses => (OrderStatus[])Enum.GetValues(typeof(OrderStatus));
    [ObservableProperty] 
    private OrderStatus status = OrderStatus.Принят;
    
    public Order(string clientName, string itemName, DateTime deliveryDate, decimal price, decimal materialCost)
    {
        ClientName = clientName;
        ItemName = itemName;
        OrderDate = DateTime.Now;
        DeliveryDate = deliveryDate;
        Price = price;
        MaterialCost = materialCost;
    }
    public Order() { }
}
