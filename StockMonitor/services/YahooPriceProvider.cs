using Microsoft.Extensions.Logging;
using StockMonitor.Interfaces;
using YahooFinanceApi;
using Polly; 
using Polly.Retry;

namespace StockMonitor.Services
{
    public class YahooPriceProvider : IPriceProvider
    {
        private readonly ILogger<YahooPriceProvider> _logger;
        // Definimos nossa política de retry
        private readonly AsyncRetryPolicy _retryPolicy;

        public YahooPriceProvider(ILogger<YahooPriceProvider> logger)
        {
            _logger = logger;
            _retryPolicy = Policy
                .Handle<Exception>(ex => ex is not KeyNotFoundException) 
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Falha ao buscar preço (Tentativa {RetryCount}). Esperando {TimeSpan}s...", retryCount, timeSpan.TotalSeconds);
                });
        }

        public async Task<decimal> GetPriceAsync(string ticker)
        {   
            ticker = ticker.ToUpperInvariant();
            try
            {
                decimal price = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var securities = await Yahoo.Symbols(ticker).Fields(Field.RegularMarketPrice).QueryAsync();
                    if (securities.TryGetValue(ticker, out var security) && security != null && security.RegularMarketPrice > 0)
                    {
                        return (decimal)security.RegularMarketPrice;
                    }
                    else
                    {
                        throw new KeyNotFoundException($"O ticker '{ticker}' é inválido ou não foi encontrado.");
                    }
                });

                return price;
            }
            catch (Exception)
            {
                _logger.LogError("Não foi possível obter cotação para {Ticker} após 3 tentativas.", ticker);
                throw; // Relança para o Worker tratar (que vai esperar 60s)
            }
        }
    }
}