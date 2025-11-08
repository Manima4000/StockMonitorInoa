using Microsoft.Extensions.Logging;
using StockMonitor.Interfaces;
using YahooFinanceApi;
using Polly;
using Polly.Retry;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace StockMonitor.Services
{
    public class YahooPriceProvider : IPriceProvider
    {
        private readonly ILogger<YahooPriceProvider> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public YahooPriceProvider(ILogger<YahooPriceProvider> logger)
        {
            _logger = logger;
            _retryPolicy = Policy
                .Handle<Exception>(ex => ex is not KeyNotFoundException)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Falha ao buscar preços (Tentativa {RetryCount}). Esperando {TimeSpan}s...", retryCount, timeSpan.TotalSeconds);
                });
        }

        public async Task<decimal> GetPriceAsync(string ticker)
        {
            var result = await GetPricesAsync([ticker]);
            if (result.TryGetValue(ticker, out var price))
            {
                return price;
            }
            throw new KeyNotFoundException($"O ticker '{ticker}' é inválido ou não foi encontrado.");
        }

        public async Task<IReadOnlyDictionary<string, decimal>> GetPricesAsync(IEnumerable<string> tickers)
        {
            var upperCaseTickers = tickers.Select(t => t.ToUpperInvariant()).ToArray();
            if (upperCaseTickers.Length == 0)
            {
                return new Dictionary<string, decimal>();
            }

            try
            {
                var prices = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var securities = await Yahoo.Symbols(upperCaseTickers).Fields(Field.RegularMarketPrice).QueryAsync();
                    var result = new Dictionary<string, decimal>();

                    foreach (var ticker in upperCaseTickers)
                    {
                        if (securities.TryGetValue(ticker, out var security) && security != null && security.RegularMarketPrice > 0)
                        {
                            result[ticker] = (decimal)security.RegularMarketPrice;
                        }
                        else
                        {
                            _logger.LogWarning("Ticker '{Ticker}' não encontrado na resposta da API.", ticker);
                        }
                    }
                    
                    if(result.Count != upperCaseTickers.Length)
                    {
                        var notFound = upperCaseTickers.Except(result.Keys);
                        _logger.LogWarning("Não foi possível obter cotação para os seguintes tickers: {Tickers}", string.Join(", ", notFound));
                    }

                    return result;
                });

                return prices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Não foi possível obter cotações para {Tickers} após 3 tentativas.", string.Join(", ", upperCaseTickers));
                // Retornar um dicionário vazio em caso de falha total para não parar o monitoramento de outros ativos.
                return new Dictionary<string, decimal>();
            }
        }
    }
}