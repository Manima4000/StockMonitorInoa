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

namespace StockMonitor.Tests;

public class StockMonitorWorkerTests
{
    private readonly Mock<ILogger<StockMonitorWorker>> _mockLogger;
    private readonly Mock<IPriceProvider> _mockPriceProvider;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IAlertingEngine> _mockAlertingEngine;
    private readonly MonitorSettings _monitorSettings;
    private readonly Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;

    public StockMonitorWorkerTests()
    {
        _mockLogger = new Mock<ILogger<StockMonitorWorker>>();
        _mockPriceProvider = new Mock<IPriceProvider>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockAlertingEngine = new Mock<IAlertingEngine>();
        _monitorSettings = new MonitorSettings("PETR4", 30, 25);
        _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlertingEngineReturnsSendSell_ShouldSendNotification()
    {
        // Arrange
        var price = 31m;
        _mockPriceProvider.Setup(p => p.GetPriceAsync(It.IsAny<string>())).ReturnsAsync(price);
        _mockAlertingEngine.Setup(e => e.CheckPrice(price)).Returns(AlertDecision.SendSell);

        var worker = new StockMonitorWorker(
            _mockLogger.Object,
            _mockPriceProvider.Object,
            _mockNotificationService.Object,
            _monitorSettings,
            _mockAlertingEngine.Object,
            _mockHostApplicationLifetime.Object);

        var cts = new CancellationTokenSource();

        // Act
        var workerTask = worker.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert
        var expectedSubject = "Alerta de Venda - PETR4.SA";
        var expectedBody = $"O preço de PETR4.SA subiu para {price}, acima do limite de {_monitorSettings.SellPrice}";

        _mockNotificationService.Verify(
            n => n.SendNotificationAsync(expectedSubject, expectedBody),
            Times.Once);

        await workerTask;
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlertingEngineReturnsSendBuy_ShouldSendNotification()
    {
        // Arrange
        var price = 24m;
        _mockPriceProvider.Setup(p => p.GetPriceAsync(It.IsAny<string>())).ReturnsAsync(price);
        _mockAlertingEngine.Setup(e => e.CheckPrice(price)).Returns(AlertDecision.SendBuy);

        var worker = new StockMonitorWorker(
            _mockLogger.Object,
            _mockPriceProvider.Object,
            _mockNotificationService.Object,
            _monitorSettings,
            _mockAlertingEngine.Object,
            _mockHostApplicationLifetime.Object);

        var cts = new CancellationTokenSource();

        // Act
        var workerTask = worker.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert
        var expectedSubject = "Alerta de Compra - PETR4.SA";
        var expectedBody = $"O preço de PETR4.SA caiu para {price}, abaixo do limite de {_monitorSettings.BuyPrice}";

        _mockNotificationService.Verify(
            n => n.SendNotificationAsync(expectedSubject, expectedBody),
            Times.Once);

        await workerTask;
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlertingEngineReturnsHold_ShouldNotSendNotification()
    {
        // Arrange
        var price = 28m;
        _mockPriceProvider.Setup(p => p.GetPriceAsync(It.IsAny<string>())).ReturnsAsync(price);
        _mockAlertingEngine.Setup(e => e.CheckPrice(price)).Returns(AlertDecision.Hold);

        var worker = new StockMonitorWorker(
            _mockLogger.Object,
            _mockPriceProvider.Object,
            _mockNotificationService.Object,
            _monitorSettings,
            _mockAlertingEngine.Object,
            _mockHostApplicationLifetime.Object);

        var cts = new CancellationTokenSource();

        // Act
        var workerTask = worker.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert
        _mockNotificationService.Verify(
            n => n.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        await workerTask;
    }

    [Fact]
    public async Task ExecuteAsync_WhenPriceProviderThrowsException_ShouldLogError()
    {
        // Arrange
        var exception = new Exception("Test Exception");
        _mockPriceProvider.Setup(p => p.GetPriceAsync(It.IsAny<string>())).ThrowsAsync(exception);

        var worker = new StockMonitorWorker(
            _mockLogger.Object,
            _mockPriceProvider.Object,
            _mockNotificationService.Object,
            _monitorSettings,
            _mockAlertingEngine.Object,
            _mockHostApplicationLifetime.Object);

        var cts = new CancellationTokenSource();

        // Act
        var workerTask = worker.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Erro no loop de monitoramento. Tentando novamente em 60s.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        await workerTask;
    }

    [Fact]
    public async Task ExecuteAsync_WhenPriceProviderThrowsKeyNotFoundException_ShouldLogErrorAndStopApplication()
    {
        // Arrange
        var exception = new KeyNotFoundException("Test KeyNotFoundException");
        _mockPriceProvider.Setup(p => p.GetPriceAsync(It.IsAny<string>())).ThrowsAsync(exception);

        var worker = new StockMonitorWorker(
            _mockLogger.Object,
            _mockPriceProvider.Object,
            _mockNotificationService.Object,
            _monitorSettings,
            _mockAlertingEngine.Object,
            _mockHostApplicationLifetime.Object);

        var cts = new CancellationTokenSource();

        // Act
        var workerTask = worker.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert
        // Verificar se o LogError foi chamado com a mensagem certa
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("inválido ou não foi encontrado")),
                null, // A exceção não é passada para o log neste caso específico
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        // Verificar se o StopApplication foi chamado
        _mockHostApplicationLifetime.Verify(h => h.StopApplication(), Times.Once);

        await workerTask;
    }
}