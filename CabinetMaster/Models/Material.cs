using CommunityToolkit.Mvvm.ComponentModel;

namespace CabinetMaster.Models;

public partial class Material : ObservableObject
{
    [ObservableProperty] private decimal pricePerUnit;
    [ObservableProperty] private decimal quantityInStock;
    public int Id { get; set; }
    public string MaterialName { get; set; }
    public string Unit { get; set; }
}