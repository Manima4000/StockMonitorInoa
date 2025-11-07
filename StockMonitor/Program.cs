using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockMonitor.Interfaces;
using StockMonitor.Services;
using StockMonitor.Settings;
using StockMonitor.Workers;
using DotNetEnv;

public class Program
{
    public static async Task Main(string[] args)
    {   
        Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
        if (args.Length != 3)
        {
            Console.WriteLine("Uso (na pasta /StockMonitor/): dotnet run -- <Ativo> <PrecoVenda> <PrecoCompra>");
            return;
        }

        // 1. Parse dos argumentos da linha de comando
        var monitorSettings = ParseMonitorSettings(args);
        if (monitorSettings == null) return;

        // 2. Criação do Host Genérico (padrão .NET moderno)
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                // 3. Carrega SmtpSettings do appsettings.json e User Secrets
                services.Configure<SmtpSettings>(hostContext.Configuration.GetSection("SmtpSettings"));

                // 4. Registra os argumentos (MonitorSettings) como um singleton
                services.AddSingleton(monitorSettings);

                // 5. Injeção de Dependência: Registra nossas classes
                // Quando alguém pedir um "IPriceProvider", entregue um "YahooPriceProvider"
                services.AddSingleton<IPriceProvider, YahooPriceProvider>();
                // Quando alguém pedir um "INotificationService", entregue um "EmailNotificationService"
                services.AddSingleton<INotificationService, EmailNotificationService>();

                services.AddSingleton<IAlertingEngine, AlertingEngine>();

                services.AddSingleton<IChartService, SpectreChartService>();

                // 6. Registra nosso loop principal como um serviço de background
                services.AddHostedService<StockMonitorWorker>();
            })
            .Build();

        // 7. Roda a aplicação
        await host.RunAsync();
    }

    private static MonitorSettings? ParseMonitorSettings(string[] args)
    {
        try
        {
            string ticker = args[0];
            decimal sellPrice = decimal.Parse(args[1], CultureInfo.InvariantCulture);
            decimal buyPrice = decimal.Parse(args[2], CultureInfo.InvariantCulture);

            return new MonitorSettings(ticker, sellPrice, buyPrice);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar argumentos: {ex.Message}");
            return null;
        }
    }
}