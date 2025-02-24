using CorteosTestTask.BackgroundServices;
using CorteosTestTask.Entity;
using CorteosTestTask.IService;
using CorteosTestTask.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Text;
using System.Xml.Linq;




public class Program
{
    static async Task Main(string[] args)
    {

        Log.Logger = new LoggerConfiguration().WriteTo.File("C:\\logs\\corteosLog.txt", rollingInterval: RollingInterval.Day).CreateLogger();
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog();
        });
        ILogger<RateService> logger = loggerFactory.CreateLogger<RateService>();
        IRateService rate = new RateService(logger);
        var backgroundService = new LoadRatesBackgroundService(rate);

        Console.Clear();

        await Task.Run(() => backgroundService.StartAsync(CancellationToken.None));

        while (true)
        {
            try
            {
                Console.WriteLine("======================= Выберите действие =======================");
                Console.Write("(1) - Список валют (2) - Курс валюты : ");
                int choice = Convert.ToInt32(Console.ReadLine());
                Console.Clear();

                if (choice == 1)
                {
                    var rates = await rate.GetAllRate(); 
                    foreach (var item in rates)
                    {
                        Console.WriteLine($"{item.Code} - {item.Name}");
                    }
                }
                else if (choice == 2)
                {
                    while (true)
                    {
                        Console.Write("Введите код валюты, например (eur): ");
                        string? nameRate = Console.ReadLine();

                        var singleRate = await rate.GetSingleRate(nameRate!); 
                        if (singleRate.Count <= 0)
                        {
                            Console.WriteLine("Валюта не найдена, попробуйте другую");
                        }
                        else
                        {
                            foreach (var item in singleRate)
                                Console.WriteLine($"{item.Date} -- {Math.Round(item.Value, 2)}");
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Что-то пошло не так... Попробуйте снова");
            }
        }
    }
}









