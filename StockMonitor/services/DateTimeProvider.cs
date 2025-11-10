using StockMonitor.Interfaces;

namespace StockMonitor.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}
