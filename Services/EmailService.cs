using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace ShiftManagement.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task EnviarConfirmacionAsync(string destinatario, string codigo)
    {
        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress("ShiftManagement", _config["Email:User"]));
        mensaje.To.Add(new MailboxAddress("", destinatario));
        mensaje.Subject = $"Tu turno {codigo} fue agendado";

        mensaje.Body = new TextPart("html")
        {
            Text = $"""
                        <div style="font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:40px;border:1px solid #eee;border-radius:16px;">
                            <h2 style="color:#1a73e8;">⚡ ShiftManagement</h2>
                            <hr style="border:none;border-top:1px solid #eee;"/>
                            <p style="color:#888;font-size:14px;">Tu turno fue agendado exitosamente</p>
                            <div style="font-size:72px;font-weight:800;color:#1a1a2e;text-align:center;margin:24px 0;">{codigo}</div>
                            <div style="background:#fffbf0;border:1px solid #ffe082;border-radius:10px;padding:12px 20px;font-size:13px;color:#b07d00;text-align:center;">
                                🔔 Por favor espera a ser llamado
                            </div>
                            <p style="color:#ccc;font-size:12px;text-align:center;margin-top:24px;">{DateTime.Now:dd/MM/yyyy HH:mm}</p>
                        </div>
                    """
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_config["Email:Host"], int.Parse(_config["Email:Port"]!), SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_config["Email:User"], _config["Email:Password"]);
        await smtp.SendAsync(mensaje);
        await smtp.DisconnectAsync(true);
    }
}