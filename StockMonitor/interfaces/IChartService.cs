using System.Collections.Generic;

namespace StockMonitor.Interfaces
{
    public enum PriceChangeDirection { Up, Down, Neutral }

    public class StockTickData
    {
        public required string Ticker { get; init; }
        public decimal CurrentPrice { get; init; }
        public decimal PreviousPrice { get; init; }
        public decimal Sma { get; init; }
        public decimal SellPrice { get; init; }
        public decimal BuyPrice { get; init; }
        public PriceChangeDirection Change => CurrentPrice > PreviousPrice ? PriceChangeDirection.Up : CurrentPrice < PreviousPrice ? PriceChangeDirection.Down : PriceChangeDirection.Neutral;
    }

    public interface IChartService
    {
        void DisplayDataTable(IEnumerable<StockTickData> data);
    }
}