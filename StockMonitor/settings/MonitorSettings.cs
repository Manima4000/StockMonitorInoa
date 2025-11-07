namespace StockMonitor.Settings
{
    public class MonitorSettings
    {
        public string Ticker { get; }
        public decimal SellPrice { get; }
        public decimal BuyPrice { get; }

        public MonitorSettings(string ticker, decimal sellPrice, decimal buyPrice)
        {
            // Na api da Yahoo Finance, os ativos precisam da extensão .SA
            Ticker = ticker + ".SA";
            SellPrice = sellPrice;
            BuyPrice = buyPrice;
        }
    }
}