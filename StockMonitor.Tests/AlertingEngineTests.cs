
using Microsoft.Extensions.Logging;
using Moq;
using StockMonitor.Interfaces;
using StockMonitor.Services;
using StockMonitor.Settings;

namespace StockMonitor.Tests;

public class AlertingEngineTests
{
    private readonly Mock<ILogger<AlertingEngine>> _mockLogger;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
    private readonly MonitorSettings _settings;
    private readonly AlertingEngine _engine;
    private DateTime _now;

    public AlertingEngineTests()
    {
        _mockLogger = new Mock<ILogger<AlertingEngine>>();
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();
        _settings = new MonitorSettings("TEST", 100, 80, 5);
        _now = DateTime.Now;
        _mockDateTimeProvider.Setup(p => p.Now).Returns(() => _now);
        _engine = new AlertingEngine(_settings, _mockLogger.Object, _mockDateTimeProvider.Object);
    }

    [Fact(DisplayName = "Deve retornar 'SendSell' quando o preço está acima do limite de venda e não há cooldown")]
    public void CheckPrice_PriceAboveSellThreshold_NoCooldown_ReturnsSendSell()
    {
        // Arrange
        var price = 101;

        // Act
        var result = _engine.CheckPrice(price);


        Assert.Equal(AlertDecision.SendSell, result);
    }

    [Fact(DisplayName = "Deve retornar 'SendBuy' quando o preço está abaixo do limite de compra e não há cooldown")]
    public void CheckPrice_PriceBelowBuyThreshold_NoCooldown_ReturnsSendBuy()
    {
        var price = 79;

        var result = _engine.CheckPrice(price);

        Assert.Equal(AlertDecision.SendBuy, result);
    }

    [Fact(DisplayName = "Deve retornar 'Hold' se o preço estiver acima do limite de venda, mas dentro do cooldown")]
    public void CheckPrice_PriceAboveSellThreshold_InCooldown_ReturnsHold()
    {
        var price = 101;
        _engine.CheckPrice(price); // First alert, starts cooldown

        _now = _now.AddMinutes(1); // Still within 5-minute cooldown

        var result = _engine.CheckPrice(price);

        Assert.Equal(AlertDecision.Hold, result);
    }

    [Fact(DisplayName = "Deve retornar 'Hold' se o preço estiver abaixo do limite de compra, mas dentro do cooldown")]
    public void CheckPrice_PriceBelowBuyThreshold_InCooldown_ReturnsHold()
    {
        // Arrange
        var price = 79;
        _engine.CheckPrice(price); // First alert, starts cooldown

        _now = _now.AddMinutes(1); // Still within 5-minute cooldown

        // Act
        var result = _engine.CheckPrice(price);

        // Assert
        Assert.Equal(AlertDecision.Hold, result);
    }

    [Fact(DisplayName = "Deve retornar 'Hold' se o preço estiver entre os limites de compra e venda")]
    public void CheckPrice_PriceBetweenThresholds_ReturnsHold()
    {
        // Arrange
        var price = 90;

        // Act
        var result = _engine.CheckPrice(price);

        // Assert
        Assert.Equal(AlertDecision.Hold, result);
    }

    [Fact(DisplayName = "Deve retornar 'SendSell' se o preço estiver acima do limite de venda e o cooldown tiver expirado")]
    public void CheckPrice_PriceAboveSellThreshold_AfterCooldown_ReturnsSendSell()
    {
        var price = 101;
        _engine.CheckPrice(price); // First alert, starts cooldown

        _now = _now.AddMinutes(6); // Cooldown has expired

        var result = _engine.CheckPrice(price);

        Assert.Equal(AlertDecision.SendSell, result);
    }

    [Fact(DisplayName = "Deve retornar 'SendBuy' se o preço estiver abaixo do limite de compra e o cooldown tiver expirado")]
    public void CheckPrice_PriceBelowBuyThreshold_AfterCooldown_ReturnsSendBuy()
    {
        var price = 79;
        _engine.CheckPrice(price); // First alert, starts cooldown

        _now = _now.AddMinutes(6); // Cooldown has expired

        var result = _engine.CheckPrice(price);

        Assert.Equal(AlertDecision.SendBuy, result);
    }
}
