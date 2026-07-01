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
    [ObservableProperty] private string editButtonText = "Редактировать";
    [ObservableProperty] private bool isAddOrderOverlayVisible;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isReadOnly = true;
    [ObservableProperty] private bool showConfirmWindow;

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
        IsBusy = true;
        all_orders.Clear();
        var orders_db = await _context.Orders.ToListAsync();
        foreach (var zak in orders_db)
        {
            all_orders.Add(zak);
        }
        IsBusy = false;
    }

    [RelayCommand]
    private void ToggleEdit() // Переключение режима редактирования таблицы
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
        AddOrderContext = new AddOrderViewModel(OnAddOrderClosed);
        IsAddOrderOverlayVisible = true;
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
            if (!order.ClientName.Contains(InputClient, StringComparison.OrdinalIgnoreCase))
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

    #endregion
}