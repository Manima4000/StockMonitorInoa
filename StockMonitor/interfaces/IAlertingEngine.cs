namespace StockMonitor.Interfaces
{
    public interface IAlertingEngine
    {
        // Recebe o preço atual e as regras
        // Retorna uma decisão (Vender, Comprar ou Manter)
        AlertDecision CheckPrice(decimal currentPrice);
    }

    public enum AlertDecision
    {
        SendBuy,
        SendSell,
        Hold
    }
}