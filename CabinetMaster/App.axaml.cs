using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using CabinetMaster.ViewModels;
using CabinetMaster.Views;
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

    // MainViewModel делаем Singleton, так как он управляет главным окном и должен жить всегда
        services.AddSingleton<MainViewModel>();
    // Собираем провайдер сервисов
        _serviceProvider = services.BuildServiceProvider();
        //services.AddDbContext<CabinetMasterDbContext>(ServiceLifetime.Transient);
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