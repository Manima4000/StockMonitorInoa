using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockMonitor.Interfaces
{
    public interface IPriceProvider
    {
        Task<decimal> GetPriceAsync(string ticker);
        Task<IReadOnlyDictionary<string, decimal>> GetPricesAsync(IEnumerable<string> tickers);
    }
}