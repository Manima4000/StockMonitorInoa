using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockMonitor.Interfaces;
using StockMonitor.Services;
using StockMonitor.Settings;
using StockMonitor.Workers;
using DotNetEnv;
using Microsoft.Extensions.Options;

public class Program
{
    public static async Task Main(string[] args)
    {
        Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Carrega SmtpSettings e a lista de Stocks do appsettings.json
                services.Configure<SmtpSettings>(hostContext.Configuration.GetSection("SmtpSettings"));
                services.Configure<List<MonitorSettings>>(hostContext.Configuration.GetSection("Stocks"));

                // Validação para garantir que a lista de ações foi configurada
                var sp = services.BuildServiceProvider();
                var stocks = sp.GetService<IOptions<List<MonitorSettings>>>()?.Value;
                if (stocks == null || stocks.Count == 0)
                {
                    Console.WriteLine("A lista de ações ('Stocks') não foi encontrada ou está vazia no arquivo appsettings.json. A aplicação não pode continuar.");
                    Environment.Exit(1);
                }


                // Injeção de Dependência: Registra nossas classes
                services.AddSingleton<IPriceProvider, YahooPriceProvider>();
                services.AddSingleton<INotificationService, EmailNotificationService>();
                services.AddSingleton<IChartService, SpectreChartService>();
                services.AddSingleton<ITechnicalAnalysisService, TechnicalAnalysisService>();

                // Registra nosso loop principal como um serviço de background
                services.AddHostedService<StockMonitorWorker>();
            })
            .Build();

        await host.RunAsync();
    }
}