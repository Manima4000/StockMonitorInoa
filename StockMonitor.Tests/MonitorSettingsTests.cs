using StockMonitor.Settings;
using System;
using Xunit;

namespace StockMonitor.Tests
{
    public class MonitorSettingsTests
    {
            [Fact(DisplayName = "Deve lançar exceção quando o preço de venda é menor que o preço de compra")]
            public void Constructor_WhenSellPriceIsLessThanBuyPrice_ShouldThrowArgumentException()
            {
                var ticker = "PETR4";
                var sellPrice = 10;
                var buyPrice = 20;
                var smaPeriod = 5;
        
                Assert.Throws<ArgumentException>(() => new MonitorSettings(ticker, sellPrice, buyPrice, smaPeriod));
            }
        
            [Fact(DisplayName = "Deve lançar exceção quando o preço de venda é igual ao preço de compra")]
            public void Constructor_WhenSellPriceIsEqualToBuyPrice_ShouldThrowArgumentException()
            { 
                var ticker = "PETR4";
                var sellPrice = 20;
                var buyPrice = 20;
                var smaPeriod = 5;

                Assert.Throws<ArgumentException>(() => new MonitorSettings(ticker, sellPrice, buyPrice, smaPeriod));
            }

        [Fact(DisplayName = "Não deve lançar exceção quando o preço de compra é menor que o preço de venda")]
        public void Constructor_WhenSellPriceIsGreaterThanBuyPrice_ShouldNotThrowException()
        {
            var ticker = "PETR4";
            var sellPrice = 30;
            var buyPrice = 20;
            var smaPeriod = 5;

            var exception = Record.Exception(() => new MonitorSettings(ticker, sellPrice, buyPrice, smaPeriod));

            Assert.Null(exception);
        }

        [Fact(DisplayName = "Deve lançar exceção quando o período da SMA é zero")]
        public void Constructor_WhenSmaPeriodIsZero_ShouldThrowArgumentException()
        {
            var ticker = "PETR4";
            var sellPrice = 30;
            var buyPrice = 20;
            var smaPeriod = 0;

            Assert.Throws<ArgumentException>(() => new MonitorSettings(ticker, sellPrice, buyPrice, smaPeriod));
        }

        [Fact(DisplayName = "Deve lançar exceção quando o período da SMA é negativo")]
        public void Constructor_WhenSmaPeriodIsNegative_ShouldThrowArgumentException()
        {
            var ticker = "PETR4";
            var sellPrice = 30;
            var buyPrice = 20;
            var smaPeriod = -1;

            Assert.Throws<ArgumentException>(() => new MonitorSettings(ticker, sellPrice, buyPrice, smaPeriod));
        }
    }
}
