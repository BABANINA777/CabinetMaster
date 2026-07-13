using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Collections;
using CabinetMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace CabinetMaster.ViewModels;

public partial class OrdersViewModel : ViewModelBase
{
    private readonly CabinetMasterDbContext _context;
    private Order? _orderToDelete;
    
    private readonly List<OrderMaterialSpecification> _addedSpecifications = new();
    private readonly Dictionary<OrderMaterialSpecification, decimal> _originalSpecQuantities = new();
    private readonly Dictionary<Material, decimal> _originalMaterialQuantities = new();

    #region Collections & View

    private ObservableCollection<Order> all_orders { get; } = new();
    public DataGridCollectionView Orders { get; } // Представление для таблицы с поддержкой фильтрации и сортировки

    #endregion

    #region Search Properties

    [ObservableProperty] private bool isActiveClientSearch; // Состояние видимости строки поиска клиента
    [ObservableProperty] private bool isActiveOrderSearch;  // Состояние видимости строки поиска изделия
    [ObservableProperty] private string inputClient = string.Empty; // Текст поиска клиента
    [ObservableProperty] private string inputOrder = string.Empty;  // Текст поиска изделия

    #endregion

    #region UI State Properties

    [ObservableProperty] private AddOrderViewModel? addOrderContext;
    [ObservableProperty] private string editButtonText = "✏️ Редактировать";
    [ObservableProperty] private bool isAddOrderOverlayVisible;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isReadOnly = true;
    [ObservableProperty] private bool showConfirmWindow;
    [ObservableProperty] private bool showMaterialWindow = false;
    public ObservableCollection<Material> AvailableMaterials { get; } = new();
    [ObservableProperty] private Material? selectedMaterial;
    [ObservableProperty] private decimal selectedMaterialCount;
    [ObservableProperty] private bool showErrorSpecificationMassage = false;
    [ObservableProperty] private string errorSpecificationMassage;
    private Order? selectedOrder;//текущий выбраный заказ
    public Order? SelectedOrder
    {
        get => selectedOrder;
        set
        {
            if (value == null) return;
            SetProperty(ref selectedOrder, value);
        }
    }
    #endregion

    #region Constructor

    public OrdersViewModel(CabinetMasterDbContext context) // Конструктор с инициализацией БД и DataGridCollectionView
    {
        _context = context;
        Orders = new DataGridCollectionView(all_orders);
        Orders.Filter = FilterOrders;
    }

    #endregion

    #region Database Operations / Commands

    [RelayCommand]
    private async Task LoadOrdersAsync() // Загрузка всех заказов из базы данных
    {
        _context.ChangeTracker.Clear();
        IsBusy = true;
        all_orders.Clear();
        var orders_db = await _context.Orders
            .Include(o => o.Client)
            .Include(o => o.MaterialsList)
                .ThenInclude(m => m.Material)
            .ToListAsync();
        foreach (var zak in orders_db)
        {
            all_orders.Add(zak);
        }
        IsBusy = false;
    }

    [RelayCommand]
    private void ToggleEdit() // Переключение режима редактирования таблицы
    {
        if (EditButtonText == "✏️ Редактировать")
        {
            EditButtonText = "Готово";
            IsReadOnly = !IsReadOnly;
        }
        else
        {
            EditButtonText = "✏️ Редактировать";
            IsReadOnly = !IsReadOnly;
            _context.SaveChangesAsync();
        }
    }

    [RelayCommand]
    private void DeleteOrder(Order order) // Инициация процесса удаления заказа с подтверждением
    {
        _orderToDelete = order;
        ShowConfirmWindow = true;
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync() // Подтверждение удаления заказа из базы данных
    {
        if (_orderToDelete != null)
        {
            _context.Orders.Remove(_orderToDelete);
            await _context.SaveChangesAsync();

            all_orders.Remove(_orderToDelete);
            _orderToDelete = null;
        }

        ShowConfirmWindow = false;
    }

    [RelayCommand]
    private void CancelDelete() // Отмена удаления заказа
    {
        _orderToDelete = null;
        ShowConfirmWindow = false;
    }

    public async Task AddOrderToDbAsync(Order newOrder) // Добавление нового заказа в базу данных
    {
        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();
        all_orders.Add(newOrder);
    }

    private async void OnAddOrderClosed(Order? newOrder) // Событие закрытия формы добавления заказа
    {
        IsAddOrderOverlayVisible = false;
        AddOrderContext = null;

        if (newOrder != null) await AddOrderToDbAsync(newOrder);
    }

    [RelayCommand]
    private void OpenAddOrder() // Открытие формы добавления заказа
    {
        AddOrderContext = new AddOrderViewModel(_context, OnAddOrderClosed);
        IsAddOrderOverlayVisible = true;
    }

    [RelayCommand]
    private void AddMaterial()//Метод добавления материала к текущему SelectedOrder
    {
        if (SelectedOrder == null)
        {
            ShowErrorSpecificationMassage = true;
            ErrorSpecificationMassage = "Ошибка: заказ не выбран";
            return;
        }

        if (SelectedMaterial == null)
        {
            ShowErrorSpecificationMassage = true;
            ErrorSpecificationMassage = "Ошибка: материал не выбран";
            return;
        }

        if (SelectedMaterialCount <= 0)
        {
            ShowErrorSpecificationMassage = true;
            ErrorSpecificationMassage = "Ошибка: количество должно быть больше 0";
            return;
        }

        if (SelectedMaterial.QuantityInStock < SelectedMaterialCount)//логика отображения окна ошибки и текущего остатка материала
        {
            ShowErrorSpecificationMassage = true;
            ErrorSpecificationMassage = $"Ошибка: на складе осталось всего {SelectedMaterial.QuantityInStock}.\nВыберите другое количество.";
            return;
        }
        _originalMaterialQuantities.TryAdd(SelectedMaterial, SelectedMaterial.QuantityInStock);// Запоминаем исходный остаток на складе перед изменениями

        var existingSpec = SelectedOrder.MaterialsList.FirstOrDefault(m => m.MaterialId == SelectedMaterial.Id || m.Material == SelectedMaterial);
        if (existingSpec != null)
        {
            _originalSpecQuantities.TryAdd(existingSpec, existingSpec.QuantityUsed);// Запоминаем оригинальное количество в спецификации перед изменением
            existingSpec.QuantityUsed += SelectedMaterialCount;// Увеличиваем затраченное количество в существующей спецификации
        }
        else
        {
            var newSpec = new OrderMaterialSpecification(SelectedOrder, SelectedMaterial, SelectedMaterialCount);// Создаем новую спецификацию
            _addedSpecifications.Add(newSpec);// Запоминаем, что спецификация была создана в этой сессии
            SelectedOrder.MaterialsList.Add(newSpec);
            _context.Entry(newSpec).State = EntityState.Added; // Безопасное добавление без каскада
        }
        // Безопасное обновление количества: ищем отслеживаемый экземпляр, чтобы избежать конфликтов
        var trackedMaterial = _context.Materials.Local.FirstOrDefault(m => m.Id == SelectedMaterial.Id);
        if (trackedMaterial != null)
        {
            trackedMaterial.QuantityInStock -= SelectedMaterialCount;
        }
        else
        {
            SelectedMaterial.QuantityInStock -= SelectedMaterialCount;
            _context.Update(SelectedMaterial);
        }

        // Пересчитываем общую себестоимость материалов для заказа
        SelectedOrder.MaterialCost = SelectedOrder.MaterialsList.Sum(m => m.TotalCost);
        ShowErrorSpecificationMassage = false;
    }

    [RelayCommand]
    private async Task SaveMaterialsAsync()
    {
        IsBusy = true;
        try
        {
            await _context.SaveChangesAsync();
            
            _addedSpecifications.Clear();
            _originalSpecQuantities.Clear();
            _originalMaterialQuantities.Clear();
            
            ShowMaterialWindow = false;
        }
        catch (Exception ex)
        {
            ShowErrorSpecificationMassage = true;
            ErrorSpecificationMassage = $"Ошибка при сохранении: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CancelMaterials()
    {
        if (SelectedOrder != null)
        {
            // 1. Откатываем новые добавленные в сессии спецификации
            foreach (var spec in _addedSpecifications)
            {
                SelectedOrder.MaterialsList.Remove(spec);
                _context.Entry(spec).State = EntityState.Detached; // Отсоединяем от трекера EF Core
            }

            // 2. Откатываем измененные количества в существующих спецификациях
            foreach (var kvp in _originalSpecQuantities)
            {
                kvp.Key.QuantityUsed = kvp.Value;
            }
        }

        // 3. Откатываем остатки на складе
        foreach (var kvp in _originalMaterialQuantities)
        {
            var trackedMaterial = _context.Materials.Local.FirstOrDefault(m => m.Id == kvp.Key.Id);
            if (trackedMaterial != null)
            {
                trackedMaterial.QuantityInStock = kvp.Value;
            }
            else
            {
                kvp.Key.QuantityInStock = kvp.Value;
            }
        }

        // 4. Пересчитываем общую себестоимость к исходному состоянию
        if (SelectedOrder != null)
        {
            SelectedOrder.MaterialCost = SelectedOrder.MaterialsList.Sum(m => m.TotalCost);
        }

        _addedSpecifications.Clear();
        _originalSpecQuantities.Clear();
        _originalMaterialQuantities.Clear();

        ShowMaterialWindow = false;
        ShowErrorSpecificationMassage = false;
    }
    #endregion

    #region Search / Filtering Commands & Predicates

    // Общий предикат фильтрации для DataGridCollectionView
    private bool FilterOrders(object obj) // Проверка соответствия заказа активным фильтрам
    {
        if (obj is not Order order) return false;

        // Фильтрация по клиенту (подстрока без учета регистра)
        if (IsActiveClientSearch && !string.IsNullOrWhiteSpace(InputClient))
        {
            if (order.Client == null || !order.Client.ClientName.Contains(InputClient, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // Фильтрация по названию изделия (подстрока без учета регистра)
        if (IsActiveOrderSearch && !string.IsNullOrWhiteSpace(InputOrder))
        {
            if (!order.ItemName.Contains(InputOrder, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    [RelayCommand]
    private void ActiveSearchClientMode() // Переключение режима поиска по клиенту
    {
        if (IsActiveClientSearch == false)
        {
            IsActiveClientSearch = true;
        }
        else
        {
            IsActiveClientSearch = false;
            InputClient = string.Empty; // сброс текста поиска при закрытии
            Orders.Refresh();
        }
    }

    [RelayCommand]
    private void ActiveSearchOrderMode() // Переключение режима поиска по изделию
    {
        if (IsActiveOrderSearch == false)
        {
            IsActiveOrderSearch = true;
        }
        else
        {
            IsActiveOrderSearch = false;
            InputOrder = string.Empty; // сброс текста поиска при закрытии
            Orders.Refresh();
        }
    }
    private async Task LoadAvailableMaterialsAsync()//Загружает материалы со складо кол-во которых больше 0
    {
        var materialsList = await _context.Materials.ToListAsync();
        foreach (var material in materialsList)
        {
            if (_context.Entry(material).State != EntityState.Detached)
            {
                await _context.Entry(material).ReloadAsync();
            }
        }
        AvailableMaterials.Clear();
        foreach (var material in materialsList)
        {
            if (material.QuantityInStock > 0)
            { AvailableMaterials.Add(material); }
        }
    }

    #endregion

    #region Property Change Handlers

    partial void OnInputClientChanged(string? value) // Автоматический вызов при изменении строки поиска клиента
    {
        Orders.Refresh();
    }

    partial void OnInputOrderChanged(string? value) // Автоматический вызов при изменении строки поиска изделия
    {
        Orders.Refresh();
    }

    partial void OnShowMaterialWindowChanged(bool value)// Автоматический вызов при изменении таблицы спецификации
    {
        if (value)
        {
            _ = LoadAvailableMaterialsAsync();
        }
    }
    
    #endregion
}