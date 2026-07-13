using CommunityToolkit.Mvvm.ComponentModel;

namespace CabinetMaster.Models;

public partial class OrderMaterialSpecification: ObservableObject
{
    public int Id { get; set; }
    
    // Внешний ключ и навигационное свойство на Заказ
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    
    // Внешний ключ и навигационное свойство на Материал
    public int MaterialId { get; set; }
    public Material? Material { get; set; }
    
    // Физические данные спецификации
    [ObservableProperty] private decimal quantityUsed; // Затраченное количество
    public decimal TotalCost => (Material?.PricePerUnit ?? 0) * QuantityUsed;// Вычисляемое свойство: Стоимость данной позиции (Кол-во * Цену за единицу)

    public OrderMaterialSpecification()
    {
    }

    public OrderMaterialSpecification(Order order, Material material, decimal quantityUsed)
    {
        Order = order;
        Material = material;
        QuantityUsed = quantityUsed;
    }
}
