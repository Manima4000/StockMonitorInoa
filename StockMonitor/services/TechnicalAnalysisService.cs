using StockMonitor.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace StockMonitor.Services
{
    public class TechnicalAnalysisService : ITechnicalAnalysisService
    {
        public decimal CalculateSma(IEnumerable<decimal> priceHistory, int period)
        {
            if (priceHistory == null || period <= 0)
            {
                return 0;
            }

            var dataForSma = priceHistory.TakeLast(period).ToList();

            if (dataForSma.Count < period)
            {
                return 0; // Not enough data points for a meaningful average
            }

            return dataForSma.Average();
        }
    }
}
