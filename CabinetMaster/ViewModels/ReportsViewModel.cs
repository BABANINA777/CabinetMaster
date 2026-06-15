using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CabinetMaster.Models;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace CabinetMaster.ViewModels;

public partial class ReportsViewModel : ViewModelBase
{
    //контекст для работы с базой данных
    private readonly CabinetMasterDbContext _context;
    [ObservableProperty] private int activeOrdersCount; // Заказы в производстве и принятые
    [ObservableProperty] private int completedOrdersCount; // Количество завершенных заказов

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private decimal netProfit; // Чистая прибыль
    [ObservableProperty] private decimal totalCosts; // Общие издержки — сумма всех MaterialCost в заказах

    //поля и свойства хранят инфу про общие значения со всех таблиц
    [ObservableProperty] private decimal totalRevenue; // Общая выручка — сумма всех продаж

    public ReportsViewModel(CabinetMasterDbContext context)
    {
        _context = context;
    }

    [RelayCommand]
    public async Task LoadReportAsync()
    {
        IsBusy = true;
        var orders = await _context.Orders.ToListAsync();

        var sumRevenue = new List<decimal>();
        foreach (var order in orders)
            if (order.Price == null)
                sumRevenue.Add(0);
            else
                sumRevenue.Add(order.Price.Value);

        TotalRevenue = sumRevenue.Sum();

        var sumCosts = new List<decimal>();
        foreach (var order in orders)
            if (order.MaterialCost == null)
                sumCosts.Add(0);
            else
                sumCosts.Add(order.MaterialCost.Value);

        TotalCosts = sumCosts.Sum();

        var sumProfit = new List<decimal>();
        foreach (var order in orders)
            if (order.Profit == null)
                sumProfit.Add(0);
            else
                sumProfit.Add(order.Profit.Value);

        NetProfit = sumProfit.Sum();

        CompletedOrdersCount = orders.Count(order => order.Status == OrderStatus.Готов);

        ActiveOrdersCount = orders.Count(order => order.Status == OrderStatus.Принят);
        ActiveOrdersCount += orders.Count(order => order.Status == OrderStatus.В_Производстве);
        IsBusy = false;
    }

    // логика сохранения в exel таблицу
    [RelayCommand]
    public async Task ExportToExcelAsync(Window window)
    {
        if (window == null) return;
        var storageProvider = TopLevel.GetTopLevel(window)?.StorageProvider;
        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить отчет",
            SuggestedFileName = "Финансовый_Отчет.xlsx",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Excel Document") { Patterns = new[] { "*.xlsx" } }
            }
        });
        if (file == null) return;
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Отчет");

        worksheet.Cell("A1").Value = "Мастерская Пилим-Красим - Финансовый отчет";

        worksheet.Cell("A3").Value = "Чистая прибыль:";
        worksheet.Cell("B3").Value = NetProfit;

        worksheet.Cell("A4").Value = "Общая выручка:";
        worksheet.Cell("B4").Value = TotalRevenue;

        worksheet.Cell("A5").Value = "Общие издержки:";
        worksheet.Cell("B5").Value = TotalCosts;

        worksheet.Column("A").AdjustToContents();
        worksheet.Column("B").AdjustToContents();

        using var stream = await file.OpenWriteAsync();
        workbook.SaveAs(stream);
    }
}