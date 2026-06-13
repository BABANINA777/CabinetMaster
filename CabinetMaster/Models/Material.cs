using System;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
namespace CabinetMaster.Models;

public partial class Material : ObservableObject
{
    public int Id { get; set; }
    public string MaterialName { get; set; }
    public string Unit { get; set; }
    [ObservableProperty] private decimal quantityInStock;
    [ObservableProperty] private decimal pricePerUnit;

}