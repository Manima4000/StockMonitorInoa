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
    private const int DefaultSmaPeriod = 5;
    public static async Task Main(string[] args)
    {   
        Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
        if (args.Length < 3 || args.Length > 4)
        {
            Console.WriteLine("USO: dotnet run -- <Ativo> <PrecoVenda> <PrecoCompra> [PeriodoMediaMovel]");
            Console.WriteLine("EXEMPLO: dotnet run -- PETR4 28.50 27.50 5");
            Console.WriteLine("\n[PeriodoMediaMovel] é opcional. O padrão é 5.");
            Console.WriteLine("\nUSO DOCKER: docker run --rm -it --env-file StockMonitor/.env stockmonitor <ATIVO> <PREÇO VENDA> <PREÇO COMPRA> [PeriodoMediaMovel]");
            return;
        }

        //Parse dos argumentos da linha de comando
        var monitorSettings = ParseMonitorSettings(args);
        if (monitorSettings == null) return;

        //Criação do Host Genérico 
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                //Carrega SmtpSettings do appsettings.json e User Secrets
                services.Configure<SmtpSettings>(hostContext.Configuration.GetSection("SmtpSettings"));

                //Registra os argumentos (MonitorSettings) como um singleton
                services.AddSingleton(monitorSettings);

                //Injeção de Dependência: Registra nossas classes
                services.AddSingleton<IPriceProvider, YahooPriceProvider>();
                services.AddSingleton<INotificationService, EmailNotificationService>();
                services.AddSingleton<IAlertingEngine, AlertingEngine>();
                services.AddSingleton<IChartService, SpectreChartService>();
                services.AddSingleton<ITechnicalAnalysisService, TechnicalAnalysisService>();

                //Registra nosso loop principal como um serviço de background
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
            string ticker = args[0].ToUpperInvariant();
            decimal sellPrice = decimal.Parse(args[1], CultureInfo.InvariantCulture);
            decimal buyPrice = decimal.Parse(args[2], CultureInfo.InvariantCulture);
            int smaPeriod = args.Length == 4 ? int.Parse(args[3]) : DefaultSmaPeriod;

            return new MonitorSettings(ticker, sellPrice, buyPrice, smaPeriod);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar argumentos: {ex.Message}");
            return null;
        }
    }
}