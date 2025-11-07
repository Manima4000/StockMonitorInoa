using Spectre.Console;
using StockMonitor.Interfaces;
using System.Collections.Generic;

namespace StockMonitor.Services
{
    public class SpectreChartService : IChartService
    {
        public void DisplayPriceChart(string ticker, List<decimal> priceHistory, decimal sellPrice, decimal buyPrice)
        {
            if (priceHistory.Count == 0) return;

            // Usa os preços de compra e venda como limites principais
            var minHistoryPrice = priceHistory.Min();
            var maxHistoryPrice = priceHistory.Max();
            
            // Define o range baseado no histórico real, não apenas em compra/venda
            var chartMinValue = Math.Min(buyPrice, minHistoryPrice);
            var chartMaxValue = Math.Max(sellPrice, maxHistoryPrice);
            
            // Adiciona uma pequena margem para melhor visualização
            var margin = (chartMaxValue - chartMinValue) * 0.05m; // 5% de margem
            if (margin < 0.01m) margin = 0.01m; // Margem mínima
            
            chartMinValue -= margin;
            chartMaxValue += margin;

            // Garante que há uma diferença mínima entre min e max
            var priceRange = chartMaxValue - chartMinValue;
            if (priceRange < 0.01m)
            {
                var midPoint = (chartMaxValue + chartMinValue) / 2;
                chartMinValue = midPoint - 0.01m;
                chartMaxValue = midPoint + 0.01m;
                priceRange = chartMaxValue - chartMinValue;
            }
            
            var chart = new BarChart()
                .Width(60)
                .Label($"[green bold underline]Histórico de Preços de {ticker}[/]")
                .CenterLabel()
                .WithMaxValue(100) // Normalizado para 0-100
                .HideValues();

            foreach (var price in priceHistory)
            {
                var color = Color.Yellow;
                if (price >= sellPrice)
                {
                    color = Color.Red;
                }
                else if (price <= buyPrice)
                {
                    color = Color.Blue;
                }

                decimal normalizedValue = (price - chartMinValue) / priceRange * 100;
                
                normalizedValue = Math.Max(0, Math.Min(100, normalizedValue));
                
                chart.AddItem(price.ToString("F2"), (double)normalizedValue, color);
            }

            var legend = new Panel(
                new Markup($"[red]≥ Venda ({sellPrice:F2})[/] | [blue]≤ Compra ({buyPrice:F2})[/] | [yellow]Normal[/]")
            );
            legend.Border = BoxBorder.None;

            var chartPanel = new Panel(chart);
            chartPanel.Header = new PanelHeader($"Range: {chartMinValue:F2} - {chartMaxValue:F2} | Venda: {sellPrice:F2} | Compra: {buyPrice:F2}");
            chartPanel.Border = BoxBorder.None;

            AnsiConsole.Write(new Rows(chartPanel, legend));
        }
    }
}