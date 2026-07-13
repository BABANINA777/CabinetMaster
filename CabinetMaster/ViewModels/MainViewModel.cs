using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CabinetMaster.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase currentPage;

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

    // Экземпляры модулей (экранов)
    public OrdersViewModel OrdersPage { get; set; }
    public WarehouseViewModel WarehousePage { get; set; }
    public ClientsViewModel ClientsPage { get; set; }
    public ReportsViewModel ReportsPage { get; set; }

    // Свойства-флаги: сообщают интерфейсу, какая вкладка сейчас открыта
    public bool IsOrdersActive => CurrentPage == OrdersPage;

    public bool IsWarehouseActive => CurrentPage == WarehousePage;
    public bool IsClientsActive => CurrentPage == ClientsPage;
    public bool IsReportsActive => CurrentPage == ReportsPage;

    [RelayCommand]
    public void ChangeModule(ViewModelBase targetPage)
    {
        if (targetPage != null)
        {
            CurrentPage = targetPage;

            // обновление данных при переходе на вкладку
            if (targetPage is ReportsViewModel reportsVM) reportsVM.LoadReportCommand.Execute(null);
            else if (targetPage is WarehouseViewModel warehouseVM) warehouseVM.LoadMaterialsCommand.Execute(null);
            else if (targetPage is OrdersViewModel ordersVM) ordersVM.LoadOrdersCommand.Execute(null);
            else if (targetPage is ClientsViewModel clientsVM) clientsVM.LoadClientsCommand.Execute(null);

            // Говорим интерфейсу обновить внешний вид кнопок
            OnPropertyChanged(nameof(IsOrdersActive));
            OnPropertyChanged(nameof(IsWarehouseActive));
            OnPropertyChanged(nameof(IsClientsActive));
            OnPropertyChanged(nameof(IsReportsActive));
        }
    }
}