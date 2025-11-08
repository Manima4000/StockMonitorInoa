using Microsoft.Extensions.Logging;
using Moq;
using StockMonitor.Services;

namespace StockMonitor.Tests;

public class YahooPriceProviderTests
{
    [Fact(DisplayName = "Deve retornar um preço maior que zero para um ticker válido")]
    public async Task GetPriceAsync_WithValidTicker_ShouldReturnPriceGreaterThanZero()
    {
        var mockLogger = new Mock<ILogger<YahooPriceProvider>>();
        var priceProvider = new YahooPriceProvider(mockLogger.Object);
        var ticker = "PETR4.SA"; 

        var price = await priceProvider.GetPriceAsync(ticker);

        Assert.True(price > 0);
    }

    [Fact(DisplayName = "Deve conseguir conectar à API do Yahoo Finance para um ticker válido")]
    public async Task GetPriceAsync_WithValidTicker_ShouldConnectToApiSuccessfully()
    {
        var mockLogger = new Mock<ILogger<YahooPriceProvider>>();
        var priceProvider = new YahooPriceProvider(mockLogger.Object);
        var ticker = "MSFT"; 

        var exception = await Record.ExceptionAsync(() => priceProvider.GetPriceAsync(ticker));
        Assert.Null(exception); 
    }
}
