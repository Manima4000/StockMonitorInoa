using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockMonitor.Interfaces;
using StockMonitor.Services;
using StockMonitor.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StockMonitor.Workers
{
    public class StockMonitorWorker : BackgroundService
    {
        private class TickerState
        {
            public required MonitorSettings Settings { get; init; }
            public required IAlertingEngine AlertingEngine { get; init; }
            public List<decimal> PriceHistory { get; } = new();
            public decimal PreviousPrice { get; set; } = 0;
        }

        private readonly ILogger<StockMonitorWorker> _logger;
        private readonly IPriceProvider _priceProvider;
        private readonly INotificationService _notificationService;
        private readonly IChartService _chartService;
        private readonly ITechnicalAnalysisService _technicalAnalysisService;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly Dictionary<string, TickerState> _tickerStates;

        public StockMonitorWorker(
            ILogger<StockMonitorWorker> logger,
            IPriceProvider priceProvider,
            INotificationService notificationService,
            IChartService chartService,
            ITechnicalAnalysisService technicalAnalysisService,
            IOptions<List<MonitorSettings>> monitorSettingsOptions,
            ILoggerFactory loggerFactory,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _priceProvider = priceProvider;
            _notificationService = notificationService;
            _chartService = chartService;
            _technicalAnalysisService = technicalAnalysisService;
            _hostApplicationLifetime = hostApplicationLifetime;

            _tickerStates = new Dictionary<string, TickerState>();
            foreach (var setting in monitorSettingsOptions.Value)
            {
                var alertingEngineLogger = loggerFactory.CreateLogger<AlertingEngine>();
                _tickerStates[setting.Ticker] = new TickerState
                {
                    Settings = setting,
                    AlertingEngine = new AlertingEngine(setting, alertingEngineLogger)
                };
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var tickersToFetch = _tickerStates.Keys.ToList();
                    var currentPrices = await _priceProvider.GetPricesAsync(tickersToFetch);

                    var displayData = new List<StockTickData>();

                    foreach (var state in _tickerStates.Values)
                    {
                        if (currentPrices.TryGetValue(state.Settings.Ticker, out var currentPrice))
                        {
                            // Update state
                            state.PriceHistory.Add(currentPrice);
                            int maxHistory = state.Settings.SmaPeriod * 2;
                            if (state.PriceHistory.Count > maxHistory)
                            {
                                state.PriceHistory.RemoveAt(0);
                            }

                            // Calculate SMA
                            var sma = _technicalAnalysisService.CalculateSma(state.PriceHistory, state.Settings.SmaPeriod);

                            // Check for alerts
                            var decision = state.AlertingEngine.CheckPrice(currentPrice);
                            if (decision != AlertDecision.Hold)
                            {
                                await HandleAlert(decision, state.Settings, currentPrice);
                            }
                            
                            // Prepare data for UI
                            displayData.Add(new StockTickData
                            {
                                Ticker = state.Settings.Ticker,
                                CurrentPrice = currentPrice,
                                PreviousPrice = state.PreviousPrice,
                                Sma = sma,
                                SellPrice = state.Settings.SellPrice,
                                BuyPrice = state.Settings.BuyPrice
                            });

                            state.PreviousPrice = currentPrice;
                        }
                    }

                    _chartService.DisplayDataTable(displayData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred in the monitoring loop. Retrying in 60 seconds.");
                    await Task.Delay(60000, stoppingToken);
                }

                await Task.Delay(2000, stoppingToken); // Increased delay to 2s to be gentler on the API
            }
        }

        private async Task HandleAlert(AlertDecision decision, MonitorSettings settings, decimal currentPrice)
        {
            string subject, body;
            if (decision == AlertDecision.SendSell)
            {
                subject = $"Alerta de Venda - {settings.Ticker}";
                body = $"O preço de {settings.Ticker} subiu para {currentPrice:F2}, acima do seu alvo de {settings.SellPrice:F2}.";
            }
            else // SendBuy
            {
                subject = $"Alerta de Compra - {settings.Ticker}";
                body = $"O preço de {settings.Ticker} caiu para {currentPrice:F2}, abaixo do seu alvo de {settings.BuyPrice:F2}.";
            }

            _logger.LogWarning(subject);
            await _notificationService.SendNotificationAsync(subject, body);
        }
    }
}