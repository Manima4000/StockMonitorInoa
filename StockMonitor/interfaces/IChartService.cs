using System.Collections.Generic;

namespace StockMonitor.Interfaces
{
    public interface IChartService
    {
        void DisplayPriceChart(string ticker, List<decimal> priceHistory, decimal sellPrice, decimal buyPrice, decimal sma);
    }
}