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
        
                Assert.Throws<ArgumentException>(() => new MonitorSettings(ticker, sellPrice, buyPrice));
            }
        
            [Fact(DisplayName = "Deve lançar exceção quando o preço de venda é igual ao preço de compra")]
            public void Constructor_WhenSellPriceIsEqualToBuyPrice_ShouldThrowArgumentException()
            { 
                var ticker = "PETR4";
                var sellPrice = 20;
                var buyPrice = 20;
        
                Assert.Throws<ArgumentException>(() => new MonitorSettings(ticker, sellPrice, buyPrice));
            }

        [Fact(DisplayName = "Não deve lançar exceção quando o preço de compra é menor que o preço de venda")]
        public void Constructor_WhenSellPriceIsGreaterThanBuyPrice_ShouldNotThrowException()
        {
            var ticker = "PETR4";
            var sellPrice = 30;
            var buyPrice = 20;

            var exception = Record.Exception(() => new MonitorSettings(ticker, sellPrice, buyPrice));

            Assert.Null(exception);
        }    
    }
}
