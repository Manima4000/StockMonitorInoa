using StockMonitor.Services;
using System.Collections.Generic;
using Xunit;

namespace StockMonitor.Tests
{
    public class TechnicalAnalysisServiceTests
    {
        private readonly TechnicalAnalysisService _service;

        public TechnicalAnalysisServiceTests()
        {
            _service = new TechnicalAnalysisService();
        }

        [Fact]
        public void CalculateSma_WithEnoughData_ReturnsCorrectAverage()
        {
            // Arrange
            var prices = new List<decimal> { 10, 11, 12, 13, 14 };
            int period = 5;

            // Act
            var sma = _service.CalculateSma(prices, period);

            Assert.Equal(12, sma);
        }

        [Fact]
        public void CalculateSma_WithMoreDataThanPeriod_UsesMostRecentData()
        {
            var prices = new List<decimal> { 1, 2, 3, 10, 11, 12, 13, 14 }; 
            int period = 5;

            var sma = _service.CalculateSma(prices, period);

            Assert.Equal(12, sma);
        }

        [Fact]
        public void CalculateSma_WithNotEnoughData_ReturnsZero()
        {
            var prices = new List<decimal> { 10, 11, 12 };
            int period = 5;

            var sma = _service.CalculateSma(prices, period);

            Assert.Equal(0, sma);
        }

        [Fact]
        public void CalculateSma_WithEmptyPrices_ReturnsZero()
        {
            var prices = new List<decimal>();
            int period = 5;

            var sma = _service.CalculateSma(prices, period);

            Assert.Equal(0, sma);
        }

        [Fact]
        public void CalculateSma_WithZeroPeriod_ReturnsZero()
        {
            var prices = new List<decimal> { 10, 11, 12, 13, 14 };
            int period = 0;

            var sma = _service.CalculateSma(prices, period);

            Assert.Equal(0, sma);
        }
    }
}
