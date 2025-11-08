using Spectre.Console;
using StockMonitor.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace StockMonitor.Services
{
    public class SpectreChartService : IChartService
    {
        public void DisplayDataTable(IEnumerable<StockTickData> data)
        {
            var table = new Table()
                .Title("[yellow bold]Dashboard de Monitoramento de Ações[/]")
                .Border(TableBorder.Rounded)
                .Expand();

            table.AddColumn("[bold]Ativo[/]");
            table.AddColumn(new TableColumn("[bold]Preço Atual[/]").Centered());
            table.AddColumn(new TableColumn("[bold]MMS[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Alvo Compra[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Alvo Venda[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Variação[/]").Centered());

            if (!data.Any())
            {
                table.AddRow("[grey]Aguardando dados...[/]");
            }
            else
            {
                foreach (var stock in data)
                {
                    var priceMarkup = GetPriceMarkup(stock.CurrentPrice, stock.SellPrice, stock.BuyPrice);
                    var smaMarkup = stock.Sma > 0 ? stock.Sma.ToString("F2") : "[grey]N/A[/]";
                    var directionMarkup = GetDirectionMarkup(stock.Change);

                    table.AddRow(
                        $"[bold]{stock.Ticker}[/]",
                        priceMarkup,
                        smaMarkup,
                        $"[blue]{stock.BuyPrice:F2}[/]",
                        $"[green]{stock.SellPrice:F2}[/]",
                        directionMarkup
                    );
                }
            }
            
            AnsiConsole.Clear();
            AnsiConsole.Write(table);
        }

        private static string GetPriceMarkup(decimal price, decimal sellPrice, decimal buyPrice)
        {
            if (price >= sellPrice) return $"[green bold]{price:F2}[/]";
            if (price <= buyPrice) return $"[red bold]{price:F2}[/]";
            return $"[white]{price:F2}[/]";
        }

        private static string GetDirectionMarkup(PriceChangeDirection direction)
        {
            return direction switch
            {
                PriceChangeDirection.Up => "[green bold]▲[/]",
                PriceChangeDirection.Down => "[red bold]▼[/]",
                _ => "[grey]-[/]"
            };
        }
    }
}