using System.Collections.Generic;

namespace StockMonitor.Interfaces
{
    public interface ITechnicalAnalysisService
    {
        decimal CalculateSma(IEnumerable<decimal> priceHistory, int period);
    }
}
