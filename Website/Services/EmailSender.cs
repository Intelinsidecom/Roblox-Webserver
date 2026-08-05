using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Website.Services;


public sealed class EmailSender
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public bool IsConfigured
    {
        get
        {
            var s = _config.GetSection("Email");
            return !string.IsNullOrWhiteSpace(s["SmtpHost"])
                && !string.IsNullOrWhiteSpace(s["SmtpPort"])
                && !string.IsNullOrWhiteSpace(s["From"]);
        }
    }

    public void Send(string to, string subject, string body)
    {
        var emailConfig = _config.GetSection("Email");
        var smtpHost = emailConfig["SmtpHost"] ?? throw new System.ArgumentNullException("Email:SmtpHost");
        var smtpPortStr = emailConfig["SmtpPort"] ?? throw new System.ArgumentNullException("Email:SmtpPort");
        var smtpUser = emailConfig["SmtpUser"];
        var smtpPass = emailConfig["SmtpPass"];
        var from = emailConfig["From"] ?? throw new System.ArgumentNullException("Email:From");
        var smtpPort = int.Parse(smtpPortStr);

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = string.IsNullOrEmpty(smtpUser)
                ? null
                : new NetworkCredential(smtpUser, smtpPass)
        };

        var msg = new MailMessage(from, to, subject, body);
        client.Send(msg);
    }
}

