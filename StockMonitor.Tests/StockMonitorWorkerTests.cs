using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using StockMonitor.Interfaces;
using StockMonitor.Services;
using StockMonitor.Settings;
using StockMonitor.Workers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

#nullable disable
namespace StockMonitor.Tests;

public class StockMonitorWorkerTests
{
    private readonly Mock<ILogger<StockMonitorWorker>> _mockLogger;
    private readonly Mock<IPriceProvider> _mockPriceProvider;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IAlertingEngine> _mockAlertingEngine;
    private readonly MonitorSettings _monitorSettings;
    private readonly Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;
    private readonly Mock<IChartService> _mockChartService;
    private readonly Mock<ITechnicalAnalysisService> _mockTechnicalAnalysisService;

    public StockMonitorWorkerTests()
    {
        _mockLogger = new Mock<ILogger<StockMonitorWorker>>();
        _mockPriceProvider = new Mock<IPriceProvider>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockAlertingEngine = new Mock<IAlertingEngine>();
        _monitorSettings = new MonitorSettings("PETR4", 30, 25, 5); // Added SMA period
        _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();
        _mockChartService = new Mock<IChartService>();
        _mockTechnicalAnalysisService = new Mock<ITechnicalAnalysisService>();

        // Setup chart service to accept the new method signature
        _mockChartService.Setup(c => c.DisplayPriceChart(
            It.IsAny<string>(),
            It.IsAny<List<decimal>>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>())); // New SMA parameter
    }

    [Fact(DisplayName = "Deve enviar notificação de venda quando o AlertingEngine retornar 'SendSell'")]
    public async Task ExecuteAsync_WhenAlertingEngineReturnsSendSell_ShouldSendNotification()
    {
        var price = 31m;
        _mockPriceProvider.Setup(p => p.GetPriceAsync(It.IsAny<string>())).ReturnsAsync(price);
        _mockAlertingEngine.Setup(e => e.CheckPrice(price)).Returns(AlertDecision.SendSell);

        var worker = new StockMonitorWorker(
            _mockLogger.Object,
            _mockPriceProvider.Object,
            _mockNotificationService.Object,
            _monitorSettings,
            _mockAlertingEngine.Object,
            _mockHostApplicationLifetime.Object,
            _mockChartService.Object,
            _mockTechnicalAnalysisService.Object); // Added new service

        var cts = new CancellationTokenSource();

        var workerTask = worker.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        var expectedSubject = $"Sell Alert - {_monitorSettings.Ticker}";
        var expectedBody = $"The price of {_monitorSettings.Ticker} rose to {price}, above the limit of {_monitorSettings.SellPrice}";

        _mockNotificationService.Verify(
            n => n.SendNotificationAsync(expectedSubject, expectedBody),
            Times.Once);

        await workerTask;
    }

    [Fact(DisplayName = "Deve enviar notificação de compra quando o AlertingEngine retornar 'SendBuy'")]
    public async Task ExecuteAsync_WhenAlertingEngineReturnsSendBuy_ShouldSendNotification()
    {

        var price = 24m;
        _mockPriceProvider.Setup(p => p.GetPriceAsync(It.IsAny<string>())).ReturnsAsync(price);
        _mockAlertingEngine.Setup(e => e.CheckPrice(price)).Returns(AlertDecision.SendBuy);

        var worker = new StockMonitorWorker(
            _mockLogger.Object,
            _mockPriceProvider.Object,
            _mockNotificationService.Object,
            _monitorSettings,
            _mockAlertingEngine.Object,
            _mockHostApplicationLifetime.Object,
            _mockChartService.Object,
            _mockTechnicalAnalysisService.Object); // Added new service

        var cts = new CancellationTokenSource();


        var workerTask = worker.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();


        var expectedSubject = $"Buy Alert - {_monitorSettings.Ticker}";
        var expectedBody = $"The price of {_monitorSettings.Ticker} dropped to {price}, below the limit of {_monitorSettings.BuyPrice}";

        _mockNotificationService.Verify(
            n => n.SendNotificationAsync(expectedSubject, expectedBody),
            Times.Once);

        await workerTask;
    }

    [Fact(DisplayName = "Não deve enviar notificação quando o AlertingEngine retornar 'Hold'")]
    public async Task ExecuteAsync_WhenAlertingEngineReturnsHold_ShouldNotSendNotification()
    {
        var price = 28m;
        _mockPriceProvider.Setup(p => p.GetPriceAsync(It.IsAny<string>())).ReturnsAsync(price);
        _mockAlertingEngine.Setup(e => e.CheckPrice(price)).Returns(AlertDecision.Hold);

        var worker = new StockMonitorWorker(
            _mockLogger.Object,
            _mockPriceProvider.Object,
            _mockNotificationService.Object,
            _monitorSettings,
            _mockAlertingEngine.Object,
            _mockHostApplicationLifetime.Object,
            _mockChartService.Object,
            _mockTechnicalAnalysisService.Object); // Added new service

        var cts = new CancellationTokenSource();

        var workerTask = worker.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        _mockNotificationService.Verify(
            n => n.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        await workerTask;
    }

    [Fact(DisplayName = "Deve registrar um erro quando o PriceProvider lançar uma exceção")]
    public async Task ExecuteAsync_WhenPriceProviderThrowsException_ShouldLogError()
    {
        var exception = new Exception("Test Exception");
        _mockPriceProvider.Setup(p => p.GetPriceAsync(It.IsAny<string>())).ThrowsAsync(exception);

        var worker = new StockMonitorWorker(
            _mockLogger.Object,
            _mockPriceProvider.Object,
            _mockNotificationService.Object,
            _monitorSettings,
            _mockAlertingEngine.Object,
            _mockHostApplicationLifetime.Object,
            _mockChartService.Object,
            _mockTechnicalAnalysisService.Object); // Added new service

        var cts = new CancellationTokenSource();

        var workerTask = worker.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error in the monitoring loop. Trying again in 60s.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        await workerTask;
    }

    [Fact(DisplayName = "Deve registrar um erro e parar a aplicação quando o PriceProvider lançar KeyNotFoundException")]
    public async Task ExecuteAsync_WhenPriceProviderThrowsKeyNotFoundException_ShouldLogErrorAndStopApplication()
    {
        var exception = new KeyNotFoundException("Test KeyNotFoundException");
        _mockPriceProvider.Setup(p => p.GetPriceAsync(It.IsAny<string>())).ThrowsAsync(exception);

        var worker = new StockMonitorWorker(
            _mockLogger.Object,
            _mockPriceProvider.Object,
            _mockNotificationService.Object,
            _monitorSettings,
            _mockAlertingEngine.Object,
            _mockHostApplicationLifetime.Object,
            _mockChartService.Object,
            _mockTechnicalAnalysisService.Object); // Added new service

        var cts = new CancellationTokenSource();

        var workerTask = worker.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("invalid or not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _mockHostApplicationLifetime.Verify(h => h.StopApplication(), Times.Once);

        await workerTask;
    }
}