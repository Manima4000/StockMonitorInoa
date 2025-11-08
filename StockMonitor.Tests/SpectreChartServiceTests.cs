using StockMonitor.Services;
using System.Collections.Generic;

namespace StockMonitor.Tests
{
    public class SpectreChartServiceTests
    {
        [Fact(DisplayName = "Deve exibir o gráfico de preços sem lançar exceção")]
        public void DisplayPriceChart_WithValidData_ShouldNotThrowException(){
            var chartService = new SpectreChartService();
            var priceHistory = new List<decimal> { 10, 20, 30 };

            var exception = Record.Exception(() => chartService.DisplayPriceChart("PETR4", priceHistory, 25, 15));

            Assert.Null(exception);
        }
    }
}