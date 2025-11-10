namespace StockMonitor.Interfaces;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}
