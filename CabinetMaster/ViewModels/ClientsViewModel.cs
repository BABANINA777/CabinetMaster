using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CabinetMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace CabinetMaster.ViewModels;

public partial class ClientsViewModel : ViewModelBase
{
    //список заказов
    public ObservableCollection<Client> Clients { get; } = new ObservableCollection<Client>();
    
    //логика кнопки обнавления данных
    private readonly CabinetMasterDbContext _context;
    public ClientsViewModel(CabinetMasterDbContext context)
    {
        _context = context;
    }
    
    [ObservableProperty]
    private bool isBusy;
    [RelayCommand]
    private async Task LoadClientsAsync()
    {
        IsBusy =  true;
        Clients.Clear();
        var clients_db = await _context.Clients.ToListAsync();
        foreach (var cli in clients_db)
        {
            Clients.Add(cli);
        }
        IsBusy =  false;
    }
    
    //логика кнопки редактировать
    [ObservableProperty] private bool isReadOnly = false;
    
    
    //логика для окошка удаления заказа
    [ObservableProperty] private bool showConfirmWindow = false;


    //логика отображения окошка с добавлением заказа
    [ObservableProperty] private bool isAddOrderOverlayVisible = false;
}