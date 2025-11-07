
using Microsoft.Extensions.Logging;
using Moq;
using StockMonitor.Interfaces;
using StockMonitor.Services;
using StockMonitor.Settings;

namespace StockMonitor.Tests;

public class AlertingEngineTests
{
    private readonly Mock<ILogger<AlertingEngine>> _mockLogger;
    private readonly MonitorSettings _settings;

    public AlertingEngineTests()
    {
        _mockLogger = new Mock<ILogger<AlertingEngine>>();
        _settings = new MonitorSettings("TEST", 100, 80);
    }

    [Fact]
    public void CheckPrice_WhenPriceIsAboveSellThreshold_ShouldReturnSendSell()
    {
        // Arrange
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        var currentPrice = 101;

        // Act
        var decision = engine.CheckPrice(currentPrice);

        // Assert
        Assert.Equal(AlertDecision.SendSell, decision);
    }

    [Fact]
    public void CheckPrice_WhenPriceIsAboveSellThresholdAndAlertSent_ShouldReturnHold()
    {
        // Arrange
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        engine.CheckPrice(101); // Send first alert

        // Act
        var decision = engine.CheckPrice(102);

        // Assert
        Assert.Equal(AlertDecision.Hold, decision);
    }

    [Fact]
    public void CheckPrice_WhenPriceIsBelowBuyThreshold_ShouldReturnSendBuy()
    {
        // Arrange
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        var currentPrice = 79;

        // Act
        var decision = engine.CheckPrice(currentPrice);

        // Assert
        Assert.Equal(AlertDecision.SendBuy, decision);
    }

    [Fact]
    public void CheckPrice_WhenPriceIsBelowBuyThresholdAndAlertSent_ShouldReturnHold()
    {
        // Arrange
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        engine.CheckPrice(79); // Send first alert

        // Act
        var decision = engine.CheckPrice(78);

        // Assert
        Assert.Equal(AlertDecision.Hold, decision);
    }

    [Fact]
    public void CheckPrice_WhenPriceDropsBelowSellThreshold_ShouldResetSellAlert()
    {
        // Arrange
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        engine.CheckPrice(101); // Send first sell alert

        // Act
        engine.CheckPrice(99); // Price drops
        var decision = engine.CheckPrice(101); // Price goes up again

        // Assert
        Assert.Equal(AlertDecision.SendSell, decision);
    }

    [Fact]
    public void CheckPrice_WhenPriceRisesAboveBuyThreshold_ShouldResetBuyAlert()
    {
        // Arrange
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        engine.CheckPrice(79); // Send first buy alert

        // Act
        engine.CheckPrice(81); // Price rises
        var decision = engine.CheckPrice(79); // Price drops again

        // Assert
        Assert.Equal(AlertDecision.SendBuy, decision);
    }

    [Fact]
    public void CheckPrice_WhenPriceIsBetweenThresholds_ShouldReturnHold()
    {
        // Arrange
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        var currentPrice = 90;

        // Act
        var decision = engine.CheckPrice(currentPrice);

        // Assert
        Assert.Equal(AlertDecision.Hold, decision);
    }
}
