using StockMonitor.Services;
using System.Collections.Generic;
using Xunit;
using Spectre.Console;
using Spectre.Console.Testing;

namespace StockMonitor.Tests
{
    public class SpectreChartServiceTests
    {
        [Fact(DisplayName = "Deve exibir o gráfico de preços sem lançar exceção")]
        public void DisplayPriceChart_WithValidData_ShouldNotThrowException()
        {
            var chartService = new SpectreChartService();
            var priceHistory = new List<decimal> { 10, 20, 30 };
            var sma = 20m;

            // Spectre.Console can throw errors in a non-interactive test runner.
            // We replace the static console with a TestConsole to prevent this.
            var originalConsole = AnsiConsole.Console;
            AnsiConsole.Console = new TestConsole();

            try
            {
                var exception = Record.Exception(() => chartService.DisplayPriceChart("PETR4", priceHistory, 25, 15, sma));

                Assert.Null(exception);
            }
            finally
            {
                AnsiConsole.Console = originalConsole;
            }
        }
    }
}