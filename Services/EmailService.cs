using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace ProyectoDBP.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config) => _config = config;

        public async Task EnviarCorreo(string to, string subject, string body)
        {
            var from = _config["EmailSettings:From"];
            var password = _config["EmailSettings:Password"];
            var host = _config["EmailSettings:Host"];
            var port = int.Parse(_config["EmailSettings:Port"]);

            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("Soporte", from));
            mensaje.To.Add(new MailboxAddress("", to));
            mensaje.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            mensaje.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, false);
            await client.AuthenticateAsync(from, password);
            await client.SendAsync(mensaje);
            await client.DisconnectAsync(true);
        }
    }
}
