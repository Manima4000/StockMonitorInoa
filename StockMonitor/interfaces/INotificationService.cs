namespace StockMonitor.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string subject, string body);
    }
}