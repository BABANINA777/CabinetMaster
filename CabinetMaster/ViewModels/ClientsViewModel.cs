using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using CabinetMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace CabinetMaster.ViewModels;

public partial class ClientsViewModel : ViewModelBase
{
    private readonly CabinetMasterDbContext _context;
    private Client? _clientToDelete;

    #region Collections & View

    private ObservableCollection<Client> all_clients { get; } = new();

    // Представление для таблицы с поддержкой фильтрации и сортировки
    public DataGridCollectionView Clients { get; }

    #endregion

    #region Search Properties

    [ObservableProperty] private bool isActiveClientSearch; // Состояние видимости строки поиска клиента
    [ObservableProperty] private string inputClient = string.Empty; // Текст поиска клиента

    #endregion

    #region UI State Properties

    [ObservableProperty] private string editButtonText = "Редактировать";
    [ObservableProperty] private bool isAddClientOverlayVisible;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isReadOnly = true;
    [ObservableProperty] private string newClientName = string.Empty;
    [ObservableProperty] private string newComment = string.Empty;
    [ObservableProperty] private string newPhoneNumber = string.Empty;
    [ObservableProperty] private bool showConfirmWindow;
    [ObservableProperty] private bool visibalErrorMessage;

    #endregion

    #region Constructor

    public ClientsViewModel(CabinetMasterDbContext context) // Конструктор с инициализацией БД и DataGridCollectionView
    {
        _context = context;
        Clients = new DataGridCollectionView(all_clients);
        Clients.Filter = FilterClients;
    }

    #endregion

    #region Database Operations / Commands

    [RelayCommand]
    private async Task LoadClientsAsync() // Загрузка всех клиентов из базы данных
    {
        _context.ChangeTracker.Clear();
        IsBusy = true;
        all_clients.Clear();
        var clients_db = await _context.Clients.ToListAsync();
        foreach (var cli in clients_db)
        {
            all_clients.Add(cli);
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task ToggleEditAsync() // Переключение режима редактирования таблицы
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
    private void DeleteClient(Client client) // Инициация процесса удаления клиента с подтверждением
    {
        _clientToDelete = client;
        ShowConfirmWindow = true;
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync() // Подтверждение удаления клиента из базы данных
    {
        if (_clientToDelete != null)
        {
            _context.Clients.Remove(_clientToDelete);
            await _context.SaveChangesAsync();
            
            all_clients.Remove(_clientToDelete);
            _clientToDelete = null;
        }

        ShowConfirmWindow = false;
    }

    [RelayCommand]
    private void CancelDelete() // Отмена удаления клиента
    {
        _clientToDelete = null;
        ShowConfirmWindow = false;
    }

    public async Task AddClientToDbAsync(Client newClient) // Добавление нового клиента в базу данных
    {
        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync();
        all_clients.Add(newClient);
    }

    [RelayCommand]
    private void OpenAddClientOverlay() // Открытие формы добавления нового клиента
    {
        NewClientName = string.Empty;
        NewPhoneNumber = string.Empty;
        NewComment = string.Empty;
        VisibalErrorMessage = false;
        IsAddClientOverlayVisible = true;
    }

    [RelayCommand]
    private async Task SaveClientAsync() // Сохранение нового клиента
    {
        if (string.IsNullOrWhiteSpace(NewClientName) || string.IsNullOrWhiteSpace(NewPhoneNumber))
        {
            VisibalErrorMessage = true;
            return;
        }

        var allowedChars = "0123456789+() ";
        var isValidPhone = NewPhoneNumber.All(c => allowedChars.Contains(c));
        if (!isValidPhone)
        {
            VisibalErrorMessage = true;
            return;
        }

        var newClient = new Client
        {
            ClientName = NewClientName,
            PhoneNumber = NewPhoneNumber,
            Comment = NewComment
        };

        await AddClientToDbAsync(newClient);
        IsAddClientOverlayVisible = false;
    }

    [RelayCommand]
    private void CancelClient() // Закрытие формы добавления клиента
    {
        IsAddClientOverlayVisible = false;
    }

    #endregion

    #region Search / Filtering Commands & Predicates

    private bool FilterClients(object obj) // Проверка соответствия клиента активным фильтрам
    {
        if (obj is not Client client) return false;

        // Фильтрация по имени клиента (подстрока без учета регистра)
        if (IsActiveClientSearch && !string.IsNullOrWhiteSpace(InputClient))
        {
            if (!client.ClientName.Contains(InputClient, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    [RelayCommand]
    private void ActiveSearchClientMode() // Переключение режима поиска по имени клиента
    {
        if (IsActiveClientSearch == false)
        {
            IsActiveClientSearch = true;
        }
        else
        {
            IsActiveClientSearch = false;
            InputClient = string.Empty; // сброс текста поиска при закрытии
            Clients.Refresh();
        }
    }

    #endregion

    #region Property Change Handlers

    partial void OnInputClientChanged(string? value) // Автоматический вызов при изменении строки поиска клиента
    {
        Clients.Refresh();
    }

    #endregion
}