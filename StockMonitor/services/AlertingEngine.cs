using StockMonitor.Interfaces;
using StockMonitor.Settings;
using Microsoft.Extensions.Logging;

namespace StockMonitor.Services
{
    public class AlertingEngine : IAlertingEngine
    {
        private readonly MonitorSettings _settings;
        private readonly ILogger<AlertingEngine> _logger;
        
        // O ESTADO AGORA VIVE AQUI, ISOLADO!
        private bool _sellAlertSent = false;
        private bool _buyAlertSent = false;

        public AlertingEngine(MonitorSettings settings, ILogger<AlertingEngine> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public AlertDecision CheckPrice(decimal currentPrice)
        {
            // Lógica de Venda
            if (currentPrice >= _settings.SellPrice && !_sellAlertSent)
            {
                _sellAlertSent = true;
                return AlertDecision.SendSell;
            }
            else if (currentPrice < _settings.SellPrice && _sellAlertSent)
            {
                _logger.LogInformation("Preço voltou ao normal. Resetando alerta de venda.");
                _sellAlertSent = false;
            }

            // Lógica de Compra
            if (currentPrice <= _settings.BuyPrice && !_buyAlertSent)
            {
                _buyAlertSent = true;
                return AlertDecision.SendBuy;
            }
            else if (currentPrice > _settings.BuyPrice && _buyAlertSent)
            {
                _logger.LogInformation("Preço voltou ao normal. Resetando alerta de compra.");
                _buyAlertSent = false;
            }

            return AlertDecision.Hold;
        }
    }
}