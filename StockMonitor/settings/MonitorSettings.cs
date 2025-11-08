namespace StockMonitor.Settings
{
    public class MonitorSettings
    {
        public string Ticker { get; }
        public decimal SellPrice { get; }
        public decimal BuyPrice { get; }
        public int SmaPeriod { get; }

        public MonitorSettings(string ticker, decimal sellPrice, decimal buyPrice, int smaPeriod)
        {
            if (sellPrice <= buyPrice)
            {
                throw new ArgumentException("O preço de venda deve ser maior que o preço de compra.");
            }
            
            if (smaPeriod <= 0)
            {
                throw new ArgumentException("O período da SMA deve ser um número positivo.");
            }

            // Na api da Yahoo Finance, os ativos precisam da extensão .SA
            Ticker = ticker + ".SA";
            SellPrice = sellPrice;
            BuyPrice = buyPrice;
            SmaPeriod = smaPeriod;
        }
    }
}