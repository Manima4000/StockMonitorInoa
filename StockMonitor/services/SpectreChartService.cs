using Spectre.Console;
using StockMonitor.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace StockMonitor.Services
{
    public class SpectreChartService : IChartService
    {
        public void DisplayPriceChart(string ticker, List<decimal> priceHistory, decimal sellPrice, decimal buyPrice, decimal sma)
        {
            if (priceHistory.Count == 0) return;

            var allValues = new List<decimal>(priceHistory) { sellPrice, buyPrice };
            if (sma > 0)
            {
                allValues.Add(sma);
            }

            var chartMinValue = allValues.Min();
            var chartMaxValue = allValues.Max();
            
            var margin = (chartMaxValue - chartMinValue) * 0.10m; 
            if (margin <= 0) margin = 0.1m;
            
            chartMinValue -= margin;
            chartMaxValue += margin;

            var priceRange = chartMaxValue - chartMinValue;
            if (priceRange <= 0) priceRange = 1;
            
            var chart = new BarChart()
                .Width(60)
                .Label($"[green bold underline]Histórico de Preços de {ticker}[/]")
                .CenterLabel()
                .WithMaxValue(100)
                .HideValues();

            foreach (var price in priceHistory)
            {
                var color = Color.Yellow;
                if (price >= sellPrice) color = Color.Green;
                else if (price <= buyPrice) color = Color.Red;

                decimal normalizedValue = (price - chartMinValue) / priceRange * 100;
                normalizedValue = Math.Max(0, Math.Min(100, normalizedValue));
                
                chart.AddItem(price.ToString("F2"), (double)normalizedValue, color);
            }

            if (sma > 0)
            {
                decimal normalizedSma = (sma - chartMinValue) / priceRange * 100;
                normalizedSma = Math.Max(0, Math.Min(100, normalizedSma));
                chart.AddItem($"MMS ({sma:F2})", (double)normalizedSma, Color.Blue);
            }

            var legendText = $"[green]≥ Venda ({sellPrice:F2})[/] | [red]≤ Compra ({buyPrice:F2})[/] | [yellow]Normal[/]";
            if (sma > 0)
            {
                legendText += " | [blue]Média Móvel[/]";
            }
            var legend = new Panel(new Markup(legendText));
            legend.Border = BoxBorder.None;

            var chartPanel = new Panel(chart);
            chartPanel.Header = new PanelHeader($"Range: {chartMinValue:F2} - {chartMaxValue:F2}");
            chartPanel.Border = BoxBorder.None;

            AnsiConsole.Clear();
            AnsiConsole.Write(new Rows(chartPanel, legend));
        }
    }
}