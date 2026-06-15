using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CabinetMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace CabinetMaster.ViewModels;

public partial class WarehouseViewModel : ViewModelBase
{
    //логика кнопки обнавления данных
    private readonly CabinetMasterDbContext _context;
    private Material? _materialToDelete;
    [ObservableProperty] private string editButtonText = "Редактировать";

    //логика отображения окошка с добавлением заказа
    [ObservableProperty] private bool isAddMaterialOverlayVisible;

    [ObservableProperty] private bool isBusy;

    //логика кнопки редактировать
    [ObservableProperty] private bool isReadOnly = true;

    [ObservableProperty] private string materialName;
    [ObservableProperty] private string pricePerUnitString;
    [ObservableProperty] private string quantityInStockString;


    //логика для окошка удаления заказа
    [ObservableProperty] private bool showConfirmWindow;
    [ObservableProperty] private string unit;

    [ObservableProperty] private bool visibalErrorMessage;

    public WarehouseViewModel(CabinetMasterDbContext context)
    {
        _context = context;
    }

    //список заказов
    public ObservableCollection<Material> Materials { get; } = new();

    public string[] AvailableUnits { get; } = new[]
    {
        "шт.",
        "пог. м",
        "кв. м",
        "лист",
        "комплект",
        "упаковка"
    };

    [RelayCommand]
    private async Task LoadMaterialsAsync()
    {
        IsBusy = true;
        Materials.Clear();
        var Materials_db = await _context.Materials.ToListAsync();
        foreach (var mat in Materials_db) Materials.Add(mat);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task ToggleEdit()
    {
        if (EditButtonText == "Редактировать")
        {
            EditButtonText = "Готово";
            IsReadOnly = !IsReadOnly;
        }
        else
        {
            EditButtonText = "Редактировать";
            IsReadOnly = !IsReadOnly;
            await _context.SaveChangesAsync();
        }
    }

    [RelayCommand]
    private void DeleteMaterial(Material material)
    {
        _materialToDelete = material;
        ShowConfirmWindow = true;
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync()
    {
        if (_materialToDelete != null)
        {
            _context.Materials.Remove(_materialToDelete);
            await _context.SaveChangesAsync();

            Materials.Remove(_materialToDelete);
            _materialToDelete = null;
        }

        ShowConfirmWindow = false;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        _materialToDelete = null;
        ShowConfirmWindow = false;
    }

    public async Task AddMaterialToDbAsync(Material newMaterial)
    {
        _context.Materials.Add(newMaterial);
        await _context.SaveChangesAsync();
        Materials.Add(newMaterial);
    }

    [RelayCommand]
    private void OpenAddMaterialOverlay()
    {
        IsAddMaterialOverlayVisible = true;
    }

    [RelayCommand]
    private void SaveMaterial()
    {
        var new_material = new Material();
        if (string.IsNullOrWhiteSpace(MaterialName) || string.IsNullOrWhiteSpace(Unit))
        {
            VisibalErrorMessage = true;
            return;
        }

        if (!(decimal.TryParse(QuantityInStockString, out var result1) &&
              decimal.TryParse(PricePerUnitString, out var result2)))
        {
            VisibalErrorMessage = true;
            return;
        }

        if (!(result1 > 0 && result2 > 0))
        {
            VisibalErrorMessage = true;
            return;
        }

        new_material.MaterialName = MaterialName;
        new_material.QuantityInStock = result1;
        new_material.PricePerUnit = result2;
        new_material.Unit = Unit;
        AddMaterialToDbAsync(new_material);
        IsAddMaterialOverlayVisible = false;
    }

    [RelayCommand]
    private void CancelMaterial()
    {
        IsAddMaterialOverlayVisible = false;
    }
}