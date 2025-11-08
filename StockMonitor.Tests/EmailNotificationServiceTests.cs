using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StockMonitor.Interfaces;
using StockMonitor.Services;
using StockMonitor.Settings;

namespace StockMonitor.Tests;

public class EmailNotificationServiceTests
{
    [Fact(DisplayName = "Deve registrar um erro quando a senha do SMTP está faltando")]
    public async Task SendNotificationAsync_WhenSmtpPasswordIsMissing_ShouldLogError()
    {
        var mockLogger = new Mock<ILogger<EmailNotificationService>>();
                var smtpSettings = new SmtpSettings
        {
            SmtpSenha = "", // Senha vazia
            EmailDestino = "test@test.com",
            SmtpServidor = "smtp.test.com",
            SmtpUsuario = "user@test.com"
        };
        var mockOptions = new Mock<IOptions<SmtpSettings>>();
        mockOptions.Setup(o => o.Value).Returns(smtpSettings);

        var notificationService = new EmailNotificationService(mockOptions.Object, mockLogger.Object);

        await notificationService.SendNotificationAsync("Test Subject", "Test Body");

                mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro: O segredo 'SmtpSettings:SmtpSenha' não foi configurado.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}