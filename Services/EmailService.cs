namespace ShiftManagement.Services;

using Models;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;

public class EmailService
{
    private readonly SmtpSettings _smtp;

    public EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtp = smtpSettings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(_smtp.SenderName, _smtp.SenderEmail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        email.Body = new TextPart("html")
        {
            Text = body
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_smtp.Server, _smtp.Port, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_smtp.Username, _smtp.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public async Task SendEmailRegister(string email, string name)
    {
        string message =
            $"<!DOCTYPE html>\n<html lang=\"es\">\n<head>\n    <meta charset=\"UTF-8\">\n    <title>Registro exitoso</title>\n    <style>\n        body {{\n            font-family: Arial, sans-serif;\n            background-color: #f4f6f8;\n            margin: 0;\n            padding: 0;\n        }}\n        .container {{\n            max-width: 600px;\n            margin: 40px auto;\n            background-color: #ffffff;\n            padding: 30px;\n            border-radius: 8px;\n            box-shadow: 0 2px 6px rgba(0,0,0,0.1);\n        }}\n        .header {{\n            text-align: center;\n            color: #2c3e50;\n        }}\n        .content {{\n            margin-top: 20px;\n            color: #555;\n            line-height: 1.6;\n        }}\n        .footer {{\n            margin-top: 30px;\n            font-size: 12px;\n            color: #999;\n            text-align: center;\n        }}\n        .button {{\n            display: inline-block;\n            margin-top: 20px;\n            padding: 12px 20px;\n            background-color: #3498db;\n            color: #fff;\n            text-decoration: none;\n            border-radius: 5px;\n        }}\n    </style>\n</head>\n<body>\n\n<div class=\"container\">\n    <div class=\"header\">\n        <h2>Registro exitoso</h2>\n    </div>\n\n    <div class=\"content\">\n        <p>Estimado/a <strong>{name}</strong>,</p>\n\n        <p>Nos complace informarte que tu registro ha sido realizado correctamente en nuestro sistema.</p>\n\n        <p>A partir de ahora podrás acceder a nuestros servicios y gestionar tus citas de manera fácil y rápida.</p>\n\n        <p>Si tienes alguna duda o necesitas asistencia, no dudes en contactarnos.</p>\n\n        <p>Gracias por confiar en nosotros.</p>\n\n        <p>Atentamente,<br>\n        <strong>Shift Management</strong></p>\n    </div>\n\n    <div class=\"footer\">\n        <p>Este es un mensaje automático, por favor no responder.</p>\n    </div>\n</div>\n\n</body>\n</html>";
        await SendEmailAsync(email, "Registro exitoso", message);
    }
}