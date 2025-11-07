namespace StockMonitor.Settings
{
    public class SmtpSettings
    {
        public required string EmailDestino { get; set; }
        public required string SmtpServidor { get; set; }
        public int SmtpPorta { get; set; }
        public required string SmtpUsuario { get; set; }
        public required string SmtpSenha { get; set; }
    }
}