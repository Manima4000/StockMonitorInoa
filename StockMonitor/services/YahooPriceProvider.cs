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

            // Configura a política de retry:
            // Tentar 3 vezes.
            // Esperar 2, 4, e 8 segundos entre as tentativas (exponential backoff)
            _retryPolicy = Policy
                .Handle<Exception>(ex => ex is not KeyNotFoundException) 
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, "Falha ao buscar preço (Tentativa {RetryCount}). Esperando {TimeSpan}s...", retryCount, timeSpan.TotalSeconds);
                });
        }

        public async Task<decimal> GetPriceAsync(string ticker)
        {
            // Agora, em vez de um try-catch simples, usamos a política:
            // "Execute a ação a seguir dentro da nossa política de retry"
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
                // Se o Polly desistir (após 3 falhas), ele lança a exceção original.
                _logger.LogError("Não foi possível obter cotação para {Ticker} após 3 tentativas.", ticker);
                throw; // Relança para o Worker tratar (que vai esperar 60s)
            }
        }
    }
}