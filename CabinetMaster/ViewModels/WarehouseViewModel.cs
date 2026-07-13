using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Collections;
using CabinetMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace CabinetMaster.ViewModels;

public partial class WarehouseViewModel : ViewModelBase
{
    private readonly CabinetMasterDbContext _context;
    private Material? _materialToDelete;

    #region Collections & View

    private ObservableCollection<Material> all_materials { get; } = new();

    // Представление для таблицы с поддержкой фильтрации и сортировки
    public DataGridCollectionView Materials { get; }

    public string[] AvailableUnits { get; } = new[]
    {
        "шт.",
        "пог. м",
        "кв. м",
        "лист",
        "комплект",
        "упаковка"
    };

    #endregion

    #region Search Properties

    [ObservableProperty] private bool isActiveMaterialSearch; // Состояние видимости строки поиска материала
    [ObservableProperty] private string inputMaterial = string.Empty; // Текст поиска материала

    #endregion

    #region UI State Properties

    [ObservableProperty] private string editButtonText = "Редактировать";
    [ObservableProperty] private bool isAddMaterialOverlayVisible;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isReadOnly = true;
    [ObservableProperty] private string materialName = string.Empty;
    [ObservableProperty] private string pricePerUnitString = string.Empty;
    [ObservableProperty] private string quantityInStockString = string.Empty;
    [ObservableProperty] private bool showConfirmWindow;
    [ObservableProperty] private string unit = string.Empty;
    [ObservableProperty] private bool visibalErrorMessage;

    #endregion

    #region Constructor

    public WarehouseViewModel(CabinetMasterDbContext context) // Конструктор с инициализацией БД и DataGridCollectionView
    {
        _context = context;
        Materials = new DataGridCollectionView(all_materials);
        Materials.Filter = FilterMaterials;
    }

    #endregion

    #region Database Operations / Commands

    [RelayCommand]
    private async Task LoadMaterialsAsync() // Загрузка всех материалов из базы данных
    {
        _context.ChangeTracker.Clear();
        IsBusy = true;
        all_materials.Clear();
        var Materials_db = await _context.Materials.ToListAsync();
        foreach (var mat in Materials_db)
        {
            all_materials.Add(mat);
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task ToggleEdit() // Переключение режима редактирования таблицы
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
    private void DeleteMaterial(Material material) // Инициация процесса удаления материала с подтверждением
    {
        _materialToDelete = material;
        ShowConfirmWindow = true;
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync() // Подтверждение удаления материала из базы данных
    {
        if (_materialToDelete != null)
        {
            _context.Materials.Remove(_materialToDelete);
            await _context.SaveChangesAsync();

            all_materials.Remove(_materialToDelete);
            _materialToDelete = null;
        }

        ShowConfirmWindow = false;
    }

    [RelayCommand]
    private void CancelDelete() // Отмена удаления материала
    {
        _materialToDelete = null;
        ShowConfirmWindow = false;
    }

    public async Task AddMaterialToDbAsync(Material newMaterial) // Добавление нового материала в базу данных
    {
        _context.Materials.Add(newMaterial);
        await _context.SaveChangesAsync();
        all_materials.Add(newMaterial);
    }

    [RelayCommand]
    private void OpenAddMaterialOverlay() // Открытие формы добавления нового материала
    {
        MaterialName = string.Empty;
        PricePerUnitString = string.Empty;
        QuantityInStockString = string.Empty;
        Unit = string.Empty;
        VisibalErrorMessage = false;
        IsAddMaterialOverlayVisible = true;
    }

    [RelayCommand]
    private async Task SaveMaterialAsync() // Сохранение нового материала
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
        await AddMaterialToDbAsync(new_material);
        IsAddMaterialOverlayVisible = false;
    }

    [RelayCommand]
    private void CancelMaterial() // Закрытие формы добавления материала
    {
        IsAddMaterialOverlayVisible = false;
    }

    #endregion

    #region Search / Filtering Commands & Predicates

    private bool FilterMaterials(object obj) // Проверка соответствия материала активным фильтрам
    {
        if (obj is not Material material) return false;

        // Фильтрация по названию материала (подстрока без учета регистра)
        if (IsActiveMaterialSearch && !string.IsNullOrWhiteSpace(InputMaterial))
        {
            if (!material.MaterialName.Contains(InputMaterial, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    [RelayCommand]
    private void ActiveSearchMaterialMode() // Переключение режима поиска по названию материала
    {
        if (IsActiveMaterialSearch == false)
        {
            IsActiveMaterialSearch = true;
        }
        else
        {
            IsActiveMaterialSearch = false;
            InputMaterial = string.Empty; // сброс текста поиска при закрытии
            Materials.Refresh();
        }
    }

    #endregion

    #region Property Change Handlers

    partial void OnInputMaterialChanged(string? value) // Автоматический вызов при изменении строки поиска материала
    {
        Materials.Refresh();
    }

    #endregion
}