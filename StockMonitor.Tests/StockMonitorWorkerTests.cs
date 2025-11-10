using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StockMonitor.Interfaces;
using StockMonitor.Services;
using StockMonitor.Settings;
using StockMonitor.Workers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

#nullable disable
namespace StockMonitor.Tests;

public class StockMonitorWorkerTests
{
    private readonly Mock<ILogger<StockMonitorWorker>> _mockWorkerLogger;
    private readonly Mock<IPriceProvider> _mockPriceProvider;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IChartService> _mockChartService;
    private readonly Mock<ITechnicalAnalysisService> _mockTechnicalAnalysisService;
    private readonly Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly List<MonitorSettings> _monitorSettings;

    public StockMonitorWorkerTests()
    {
        _mockWorkerLogger = new Mock<ILogger<StockMonitorWorker>>();
        _mockPriceProvider = new Mock<IPriceProvider>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockChartService = new Mock<IChartService>();
        _mockTechnicalAnalysisService = new Mock<ITechnicalAnalysisService>();
        _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();

        _mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger<AlertingEngine>>().Object);

        _monitorSettings = new List<MonitorSettings>
        {
            new("PETR4", 30, 25, 5),
            new("VALE3", 70, 65, 5)
        };
    }

    private StockMonitorWorker CreateWorker(List<MonitorSettings> settings)
    {
        var options = Options.Create(settings);
        return new StockMonitorWorker(
            _mockWorkerLogger.Object,
            _mockPriceProvider.Object,
            _mockNotificationService.Object,
            _mockChartService.Object,
            _mockTechnicalAnalysisService.Object,
            options,
            _mockLoggerFactory.Object,
            _mockHostApplicationLifetime.Object);
    }

    [Fact(DisplayName = "Deve enviar notificação de venda para o ativo correto")]
    public async Task ExecuteAsync_WhenSellPriceIsReached_ShouldSendNotificationForCorrectTicker()
    {
        var petr4 = _monitorSettings[0];
        var vale3 = _monitorSettings[1];
        var prices = new Dictionary<string, decimal>
        {
            { petr4.Ticker, 31m }, 
            { vale3.Ticker, 68m }  
        };
        _mockPriceProvider.Setup(p => p.GetPricesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(prices);

        var worker = CreateWorker(_monitorSettings);
        var cts = new CancellationTokenSource(100); 

        await worker.StartAsync(cts.Token);

        var expectedSubject = $"Alerta de Venda - {petr4.Ticker}";
        var expectedBody = $"O preço de {petr4.Ticker} subiu para {prices[petr4.Ticker]:F2}, acima do seu alvo de {petr4.SellPrice:F2}.";
        
        _mockNotificationService.Verify(
            n => n.SendNotificationAsync(expectedSubject, expectedBody),
            Times.Once);
        
        _mockNotificationService.Verify(
            n => n.SendNotificationAsync(It.Is<string>(s => s.Contains(vale3.Ticker)), It.IsAny<string>()),
            Times.Never);
    }

    [Fact(DisplayName = "Não deve enviar notificação quando os preços estão estáveis")]
    public async Task ExecuteAsync_WhenPricesAreStable_ShouldNotSendNotification()
    {
        var prices = new Dictionary<string, decimal>
        {
            { "PETR4.SA", 28m },
            { "VALE3.SA", 68m }
        };
        _mockPriceProvider.Setup(p => p.GetPricesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(prices);

        var worker = CreateWorker(_monitorSettings);
        var cts = new CancellationTokenSource(100);

        await worker.StartAsync(cts.Token);

        _mockNotificationService.Verify(
            n => n.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact(DisplayName = "Deve chamar os serviços para cada ativo com preço")]
    public async Task ExecuteAsync_WithValidPrices_ShouldCallServicesForEachTicker()
    {
        var prices = new Dictionary<string, decimal>
        {
            { "PETR4.SA", 28m },
            { "VALE3.SA", 68m }
        };
        _mockPriceProvider.Setup(p => p.GetPricesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(prices);

        var worker = CreateWorker(_monitorSettings);
        var cts = new CancellationTokenSource(100);

        await worker.StartAsync(cts.Token);

        _mockTechnicalAnalysisService.Verify(
            t => t.CalculateSma(It.IsAny<List<decimal>>(), It.IsAny<int>()),
            Times.Exactly(2));

        _mockChartService.Verify(
            c => c.DisplayDataTable(It.Is<IEnumerable<StockTickData>>(d => d.Count() == 2)),
            Times.Once);
    }
}