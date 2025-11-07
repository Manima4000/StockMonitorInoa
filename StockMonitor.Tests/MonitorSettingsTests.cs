using StockMonitor.Settings;
using System;
using Xunit;

namespace StockMonitor.Tests
{
    public class MonitorSettingsTests
    {
        [Fact]
        public void Constructor_WhenSellPriceIsLessThanBuyPrice_ShouldThrowArgumentException()
        {
            // Arrange
            var ticker = "PETR4";
            var sellPrice = 10;
            var buyPrice = 20;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new MonitorSettings(ticker, sellPrice, buyPrice));
        }

        [Fact]
        public void Constructor_WhenSellPriceIsEqualToBuyPrice_ShouldThrowArgumentException()
        { 
            // Arrange
            var ticker = "PETR4";
            var sellPrice = 20;
            var buyPrice = 20;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new MonitorSettings(ticker, sellPrice, buyPrice));
        }

        [Fact]
        public void Constructor_WhenSellPriceIsGreaterThanBuyPrice_ShouldNotThrowException()
        {
            // Arrange
            var ticker = "PETR4";
            var sellPrice = 30;
            var buyPrice = 20;

            // Act
            var exception = Record.Exception(() => new MonitorSettings(ticker, sellPrice, buyPrice));

            // Assert
            Assert.Null(exception);
        }
    }
}
