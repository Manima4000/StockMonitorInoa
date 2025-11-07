using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockMonitor.Interfaces;
using StockMonitor.Settings;

namespace StockMonitor.Workers
{
    public class StockMonitorWorker : BackgroundService
    {
        private readonly ILogger<StockMonitorWorker> _logger;
        private readonly IPriceProvider _priceProvider;
        private readonly INotificationService _notificationService;
        private readonly MonitorSettings _monitorSettings;
        private readonly IAlertingEngine _alertingEngine; // <-- NOVO
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public StockMonitorWorker(
            ILogger<StockMonitorWorker> logger,
            IPriceProvider priceProvider,
            INotificationService notificationService,
            MonitorSettings monitorSettings,
            IAlertingEngine alertingEngine,
            IHostApplicationLifetime hostApplicationLifetime) 
        {
            _logger = logger;
            _priceProvider = priceProvider;
            _notificationService = notificationService;
            _monitorSettings = monitorSettings;
            _alertingEngine = alertingEngine; 
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    decimal precoAtual = await _priceProvider.GetPriceAsync(_monitorSettings.Ticker);
                    _logger.LogInformation("Cotação atual de {Ticker}: {Preco}", _monitorSettings.Ticker, precoAtual);

                    // Delega a decisão para a classe de lógica
                    var decision = _alertingEngine.CheckPrice(precoAtual);

                    // Apenas reage à decisão
                    if (decision == AlertDecision.SendSell)
                    {
                        var subject = $"Alerta de Venda - {_monitorSettings.Ticker}";
                        var body = $"O preço de {_monitorSettings.Ticker} subiu para {precoAtual}, acima do limite de {_monitorSettings.SellPrice}";
                        await _notificationService.SendNotificationAsync(subject, body);
                    }
                    else if (decision == AlertDecision.SendBuy)
                    {
                        var subject = $"Alerta de Compra - {_monitorSettings.Ticker}";
                        var body = $"O preço de {_monitorSettings.Ticker} caiu para {precoAtual}, abaixo do limite de {_monitorSettings.BuyPrice}";
                        await _notificationService.SendNotificationAsync(subject, body);
                    }
                }
                catch (KeyNotFoundException)
                {
                    _logger.LogError("O ticker {Ticker} é inválido ou não foi encontrado. A aplicação será encerrada.", _monitorSettings.Ticker);
                    _hostApplicationLifetime.StopApplication();
                }
                catch (Exception)
                {
                    _logger.LogError("Erro no loop de monitoramento. Tentando novamente em 60s.");
                    await Task.Delay(60000, stoppingToken);
                }
                
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}