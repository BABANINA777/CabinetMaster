using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CabinetMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace CabinetMaster.ViewModels;

public partial class ClientsViewModel : ViewModelBase
{
    //логика кнопки обнавления данных
    private readonly CabinetMasterDbContext _context;
    private Client? _clientToDelete;
    [ObservableProperty] private string editButtonText = "Редактировать";

    //логика отображения окошка с добавлением заказа
    [ObservableProperty] private bool isAddClientOverlayVisible;

    [ObservableProperty] private bool isBusy;

    //логика кнопки редактировать
    [ObservableProperty] private bool isReadOnly = true;

    [ObservableProperty] private string newClientName = string.Empty;
    [ObservableProperty] private string newComment = string.Empty;
    [ObservableProperty] private string newPhoneNumber = string.Empty;

    //логика для окошка удаления заказа
    [ObservableProperty] private bool showConfirmWindow;
    [ObservableProperty] private bool visibalErrorMessage;

    public ClientsViewModel(CabinetMasterDbContext context)
    {
        _context = context;
    }

    //список клиентов
    public ObservableCollection<Client> Clients { get; } = new();

    [RelayCommand]
    private async Task LoadClientsAsync()
    {
        IsBusy = true;
        Clients.Clear();
        var clients_db = await _context.Clients.ToListAsync();
        foreach (var cli in clients_db) Clients.Add(cli);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task ToggleEditAsync()
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
    private void DeleteClient(Client client)
    {
        _clientToDelete = client;
        ShowConfirmWindow = true;
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync()
    {
        if (_clientToDelete != null)
        {
            _context.Clients.Remove(_clientToDelete);
            await _context.SaveChangesAsync();
            Clients.Remove(_clientToDelete);
            _clientToDelete = null;
        }

        ShowConfirmWindow = false;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        _clientToDelete = null;
        ShowConfirmWindow = false;
    }

    public async Task AddClientToDbAsync(Client newClient)
    {
        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync();
        Clients.Add(newClient);
    }

    [RelayCommand]
    private void OpenAddClientOverlay()
    {
        NewClientName = string.Empty;
        NewPhoneNumber = string.Empty;
        NewComment = string.Empty;
        VisibalErrorMessage = false;
        IsAddClientOverlayVisible = true;
    }

    [RelayCommand]
    private async Task SaveClientAsync()
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
    private void CancelClient()
    {
        IsAddClientOverlayVisible = false;
    }
}