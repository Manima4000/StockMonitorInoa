namespace StockMonitor.Interfaces
{
    public interface IPriceProvider
    {
        Task<decimal> GetPriceAsync(string ticker);
    }
}