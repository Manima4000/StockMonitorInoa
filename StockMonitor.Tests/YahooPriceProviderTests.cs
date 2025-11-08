using Microsoft.Extensions.Logging;
using Moq;
using StockMonitor.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace StockMonitor.Tests;

public class YahooPriceProviderTests
{
    private readonly Mock<ILogger<YahooPriceProvider>> _mockLogger;
    private readonly YahooPriceProvider _priceProvider;

    public YahooPriceProviderTests()
    {
        _mockLogger = new Mock<ILogger<YahooPriceProvider>>();
        _priceProvider = new YahooPriceProvider(_mockLogger.Object);
    }

    [Fact(DisplayName = "Deve retornar um preço maior que zero para um ticker válido")]
    public async Task GetPriceAsync_WithValidTicker_ShouldReturnPriceGreaterThanZero()
    {
        var ticker = "PETR4.SA"; 

        var price = await _priceProvider.GetPriceAsync(ticker);

        Assert.True(price > 0);
    }

    [Fact(DisplayName = "Deve conseguir conectar à API do Yahoo Finance para um ticker válido")]
    public async Task GetPriceAsync_WithValidTicker_ShouldConnectToApiSuccessfully()
    {
        var ticker = "MSFT"; 

        var exception = await Record.ExceptionAsync(() => _priceProvider.GetPriceAsync(ticker));
        
        Assert.Null(exception); 
    }

    [Fact(DisplayName = "Deve retornar preços para múltiplos tickers válidos")]
    public async Task GetPricesAsync_WithMultipleValidTickers_ShouldReturnPrices()
    {
        var tickers = new[] { "PETR4.SA", "VALE3.SA", "MGLU3.SA" };

        var prices = await _priceProvider.GetPricesAsync(tickers);

        Assert.Equal(tickers.Length, prices.Count);
        Assert.All(prices.Values, price => Assert.True(price > 0));
    }

    [Fact(DisplayName = "Deve retornar um dicionário vazio para uma lista vazia de tickers")]
    public async Task GetPricesAsync_WithEmptyList_ShouldReturnEmptyDictionary()
    {
        var tickers = Array.Empty<string>();

        var prices = await _priceProvider.GetPricesAsync(tickers);

        Assert.Empty(prices);
    }
}
