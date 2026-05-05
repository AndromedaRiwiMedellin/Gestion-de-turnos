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
        await smtp.ConnectAsync(_smtp.Server, _smtp.Port, false);
        await smtp.AuthenticateAsync(_smtp.Username, _smtp.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}