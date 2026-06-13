using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using CabinetMaster.ViewModels;
using CabinetMaster.Views;
using CabinetMaster.Models;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CabinetMaster;


public partial class App : Application
{
    private IServiceProvider _serviceProvider;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
    // Регистрируем дочерние ViewModel (Transient означает, что каждый раз будет создаваться новый экземпляр, 
    // но так как они запрашиваются только один раз в MainViewModel, этого достаточно)
        services.AddTransient<OrdersViewModel>();
        services.AddTransient<WarehouseViewModel>();
        services.AddTransient<ClientsViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddDbContext<CabinetMasterDbContext>(ServiceLifetime.Transient);
    // MainViewModel делаем Singleton, так как он управляет главным окном и должен жить всегда
        services.AddSingleton<MainViewModel>();
    // Собираем провайдер сервисов
        _serviceProvider = services.BuildServiceProvider();
        
        using (var context = _serviceProvider.GetRequiredService<CabinetMasterDbContext>())
        {
            context.Database.EnsureCreated(); // Создаст файл db и таблицы, если их еще нет
        }
        
        //Получение объекта MainViewModel, через него получение дочерних объектов вкладок и вызов их методов обновления отображения
        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        mainViewModel.OrdersPage.LoadOrdersCommand.Execute(null);
        mainViewModel.WarehousePage.LoadMaterialsCommand.Execute(null);
        mainViewModel.ClientsPage.LoadClientsCommand.Execute(null);
        
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
            };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            singleViewFactoryApplicationLifetime.MainViewFactory =
                () => new MainView { DataContext = _serviceProvider.GetRequiredService<MainViewModel>() };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}