namespace StockMonitor.Interfaces
{
    // Define um contrato para a nossa lógica de decisão
    public interface IAlertingEngine
    {
        // Recebe o preço atual e as regras
        // Retorna uma decisão (Vender, Comprar ou Manter)
        AlertDecision CheckPrice(decimal currentPrice);
    }

    // Um "enum" para as decisões
    public enum AlertDecision
    {
        SendBuy,
        SendSell,
        Hold
    }
}