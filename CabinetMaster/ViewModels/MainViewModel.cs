using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CabinetMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace CabinetMaster.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase currentPage;

    // Экземпляры модулей (экранов)
    public OrdersViewModel OrdersPage { get; set;}
    public WarehouseViewModel WarehousePage { get; set;}
    public ClientsViewModel ClientsPage { get; set;}
    public ReportsViewModel ReportsPage { get; set; }

    // Свойства-флаги: сообщают интерфейсу, какая вкладка сейчас открыта
    public bool IsOrdersActive
    {
        get
        {
            return CurrentPage == OrdersPage;
        }
    }
    public bool IsWarehouseActive => CurrentPage == WarehousePage;
    public bool IsClientsActive => CurrentPage == ClientsPage;
    public bool IsReportsActive => CurrentPage == ReportsPage;
    
    public MainViewModel(
        OrdersViewModel ordersPage, 
        WarehouseViewModel warehousePage, 
        ClientsViewModel clientsPage, 
        ReportsViewModel reportsPage)
    {
        OrdersPage = ordersPage;
        WarehousePage = warehousePage;
        ClientsPage = clientsPage;
        ReportsPage = reportsPage;

        CurrentPage = OrdersPage;
    }

    [RelayCommand]
    public void ChangeModule(ViewModelBase targetPage)
    {
        if (targetPage != null)
        {
            CurrentPage = targetPage;

            // Говорим интерфейсу обновить внешний вид кнопок
            OnPropertyChanged(nameof(IsOrdersActive));
            OnPropertyChanged(nameof(IsWarehouseActive));
            OnPropertyChanged(nameof(IsClientsActive));
            OnPropertyChanged(nameof(IsReportsActive));
        }
    }
}