using StockMonitor.Interfaces;
using StockMonitor.Settings;
using Microsoft.Extensions.Logging;

namespace StockMonitor.Services
{
    public class AlertingEngine : IAlertingEngine
    {
        private readonly MonitorSettings _settings;
        private readonly ILogger<AlertingEngine> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private DateTime _lastSellAlertTime = DateTime.MinValue;
        private DateTime _lastBuyAlertTime = DateTime.MinValue;
        private readonly TimeSpan _cooldownPeriod = TimeSpan.FromMinutes(5); // 5 minutos de cooldown

        public AlertingEngine(MonitorSettings settings, ILogger<AlertingEngine> logger, IDateTimeProvider dateTimeProvider)
        {
            _settings = settings;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
        }

        public AlertDecision CheckPrice(decimal currentPrice)
        {
            var now = _dateTimeProvider.Now;

            // Lógica de Venda
            if (currentPrice >= _settings.SellPrice)
            {
                if ((now - _lastSellAlertTime) > _cooldownPeriod)
                {
                    _lastSellAlertTime = now;
                    return AlertDecision.SendSell;
                }
            }

            // Lógica de Compra
            if (currentPrice <= _settings.BuyPrice)
            {
                if ((now - _lastBuyAlertTime) > _cooldownPeriod)
                {
                    _lastBuyAlertTime = now;
                    return AlertDecision.SendBuy;
                }
            }

            return AlertDecision.Hold;
        }
    }
}