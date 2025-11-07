using Microsoft.Extensions.Logging;
using Moq;
using StockMonitor.Services;

namespace StockMonitor.Tests;

public class YahooPriceProviderTests
{
    [Fact]
    // Isso é um teste de integração, pois ele depende de um serviço externo (API do Yahoo Finance)
    // e requer uma conexão com a internet para ser executado com sucesso.
    public async Task GetPriceAsync_WithValidTicker_ShouldReturnPriceGreaterThanZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<YahooPriceProvider>>();
        var priceProvider = new YahooPriceProvider(mockLogger.Object);
        var ticker = "PETR4.SA"; // Usando um ticker válido da B3

        // Act
        var price = await priceProvider.GetPriceAsync(ticker);

        // Assert
        Assert.True(price > 0);
    }
}
