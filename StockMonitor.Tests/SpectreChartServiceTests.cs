using StockMonitor.Services;
using System.Collections.Generic;
using Xunit;
using Spectre.Console;
using Spectre.Console.Testing;
using StockMonitor.Interfaces;

namespace StockMonitor.Tests
{
    public class SpectreChartServiceTests
    {
        [Fact(DisplayName = "Deve exibir a tabela de dados sem lançar exceção")]
        public void DisplayDataTable_WithValidData_ShouldNotThrowException()
        {
            var chartService = new SpectreChartService();
            var testData = new List<StockTickData>
            {
                new StockTickData { Ticker = "PETR4", CurrentPrice = 28.50m, PreviousPrice = 28.40m, Sma = 28.30m, SellPrice = 30, BuyPrice = 25 },
                new StockTickData { Ticker = "VALE3", CurrentPrice = 65.10m, PreviousPrice = 65.20m, Sma = 65.00m, SellPrice = 70, BuyPrice = 60 }
            };
            var originalConsole = AnsiConsole.Console;
            AnsiConsole.Console = new TestConsole();

            try
            {
                var exception = Record.Exception(() => chartService.DisplayDataTable(testData));
                Assert.Null(exception);
            }
            finally
            {
                AnsiConsole.Console = originalConsole;
            }
        }
    }
}