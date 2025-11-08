using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockMonitor.Interfaces;
using StockMonitor.Settings;
using System.Collections.Generic;

namespace StockMonitor.Workers
{
    public class StockMonitorWorker : BackgroundService
    {
        private readonly ILogger<StockMonitorWorker> _logger;
        private readonly IPriceProvider _priceProvider;
        private readonly INotificationService _notificationService;
        private readonly MonitorSettings _monitorSettings;
        private readonly IAlertingEngine _alertingEngine;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IChartService _chartService;
        private readonly ITechnicalAnalysisService _technicalAnalysisService;
        private readonly List<decimal> _priceHistory = new List<decimal>();

        public StockMonitorWorker(
            ILogger<StockMonitorWorker> logger,
            IPriceProvider priceProvider,
            INotificationService notificationService,
            MonitorSettings monitorSettings,
            IAlertingEngine alertingEngine,
            IHostApplicationLifetime hostApplicationLifetime,
            IChartService chartService,
            ITechnicalAnalysisService technicalAnalysisService) 
        {
            _logger = logger;
            _priceProvider = priceProvider;
            _notificationService = notificationService;
            _monitorSettings = monitorSettings;
            _alertingEngine = alertingEngine; 
            _hostApplicationLifetime = hostApplicationLifetime;
            _chartService = chartService;
            _technicalAnalysisService = technicalAnalysisService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    decimal currentPrice = await _priceProvider.GetPriceAsync(_monitorSettings.Ticker);
                    _logger.LogInformation("Current quote for {Ticker}: {Price}", _monitorSettings.Ticker, currentPrice);

                    _priceHistory.Add(currentPrice);
                    int maxHistoryCount = _monitorSettings.SmaPeriod * 2;
                    if (_priceHistory.Count > maxHistoryCount)
                    {
                        _priceHistory.RemoveAt(0);
                    }

                    decimal sma = _technicalAnalysisService.CalculateSma(_priceHistory, _monitorSettings.SmaPeriod);

                    _chartService.DisplayPriceChart(_monitorSettings.Ticker, _priceHistory, _monitorSettings.SellPrice, _monitorSettings.BuyPrice, sma);

                    // Delegate the decision to the logic class
                    var decision = _alertingEngine.CheckPrice(currentPrice);

                    // React to the decision
                    if (decision == AlertDecision.SendSell)
                    {
                        var subject = $"Sell Alert - {_monitorSettings.Ticker}";
                        var body = $"The price of {_monitorSettings.Ticker} rose to {currentPrice}, above the limit of {_monitorSettings.SellPrice}";
                        await _notificationService.SendNotificationAsync(subject, body);
                    }
                    else if (decision == AlertDecision.SendBuy)
                    {
                        var subject = $"Buy Alert - {_monitorSettings.Ticker}";
                        var body = $"The price of {_monitorSettings.Ticker} dropped to {currentPrice}, below the limit of {_monitorSettings.BuyPrice}";
                        await _notificationService.SendNotificationAsync(subject, body);
                    }
                }
                catch (KeyNotFoundException)
                {
                    _logger.LogError("The ticker {Ticker} is invalid or not found. The application will be shut down.", _monitorSettings.Ticker);
                    _hostApplicationLifetime.StopApplication();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in the monitoring loop. Trying again in 60s.");
                    await Task.Delay(60000, stoppingToken);
                }
                
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}