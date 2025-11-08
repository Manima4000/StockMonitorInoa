
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

    [Fact(DisplayName = "Deve retornar 'SendSell' quando o preço está acima do limite de venda")]
    public void CheckPrice_WhenPriceIsAboveSellThreshold_ShouldReturnSendSell()
    {
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        var currentPrice = 101;

        var decision = engine.CheckPrice(currentPrice);

        Assert.Equal(AlertDecision.SendSell, decision);
    }

    [Fact(DisplayName = "Deve retornar 'Hold' quando o preço está acima do limite de venda e o alerta já foi enviado")]
    public void CheckPrice_WhenPriceIsAboveSellThresholdAndAlertSent_ShouldReturnHold()
    {
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        engine.CheckPrice(101); // Send first alert

        var decision = engine.CheckPrice(102);

        Assert.Equal(AlertDecision.Hold, decision);
    }

    [Fact(DisplayName = "Deve retornar 'SendBuy' quando o preço está abaixo do limite de compra")]
    public void CheckPrice_WhenPriceIsBelowBuyThreshold_ShouldReturnSendBuy()
    {
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        var currentPrice = 79;

        var decision = engine.CheckPrice(currentPrice);

        Assert.Equal(AlertDecision.SendBuy, decision);
    }

    [Fact(DisplayName = "Deve retornar 'Hold' quando o preço está abaixo do limite de compra e o alerta já foi enviado")]
    public void CheckPrice_WhenPriceIsBelowBuyThresholdAndAlertSent_ShouldReturnHold()
    {
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        engine.CheckPrice(79); 

        var decision = engine.CheckPrice(78);

        Assert.Equal(AlertDecision.Hold, decision);
    }

    [Fact(DisplayName = "Deve resetar o alerta de venda quando o preço volta ao normal")]
    public void CheckPrice_WhenPriceDropsBelowSellThreshold_ShouldResetSellAlert()
    {
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        engine.CheckPrice(101); 

        engine.CheckPrice(99);
        var decision = engine.CheckPrice(101); 

        Assert.Equal(AlertDecision.SendSell, decision);
    }

    [Fact(DisplayName = "Deve resetar o alerta de compra quando o preço volta ao normal")]
    public void CheckPrice_WhenPriceRisesAboveBuyThreshold_ShouldResetBuyAlert()
    {
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        engine.CheckPrice(79);

        engine.CheckPrice(81); 
        var decision = engine.CheckPrice(79);

        Assert.Equal(AlertDecision.SendBuy, decision);
    }

    [Fact(DisplayName = "Deve retornar 'Hold' quando o preço está entre os limites")]
    public void CheckPrice_WhenPriceIsBetweenThresholds_ShouldReturnHold()
    {
        var engine = new AlertingEngine(_settings, _mockLogger.Object);
        var currentPrice = 90;

        var decision = engine.CheckPrice(currentPrice);

        Assert.Equal(AlertDecision.Hold, decision);
    }
}
