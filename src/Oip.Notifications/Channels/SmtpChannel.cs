using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using Oip.Base.Services;
using Oip.Notifications.Base;

namespace Oip.Notifications.Channels;
/*
public class SmtpChannel : INotificationChannel
{
    private readonly ILogger<SmtpChannel> _logger;
    private readonly CryptService _cryptService;
    public string Name { get; set; } = "Smtp client";

    public SmtpChannel(ILogger<SmtpChannel> logger,
        CryptService cryptService)
    {
        ServicePointManager.SecurityProtocol |= (SecurityProtocolType)0xC00;
        _logger = logger;
        _cryptService = cryptService;
    }

    private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
    {
        _logger.LogInformation("Message sent");
        ((SmtpClient)sender).Dispose();
    }

    /// <inheritdoc />
    public bool IsEnable { get; }

    /// <inheritdoc />
    public void OpenChannel()
    {
        // Nothing do
    }

    public void CloseChannel()
    {
        // Nothing do
    }

    public void Send(UserInfoDto userInfoDto, string subject, string message)
    {
        Send(userInfoDto, subject, message, Array.Empty<Attachment>());
    }

    /// <inheritdoc />
    public void Send(UserInfoDto userInfoDto, string subject, string message, Attachment[]? attachments)
    {
        var smtpClient = CreateSmtpClient();
        var mail = new MailMessage(Settings.MailFrom, userInfoDto.Email)
        {
            Subject = subject,
            Body = message
        };
        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                mail.Attachments.Add(attachment);
            }
        }

        try
        {
            smtpClient.Send(mail);
            _logger.LogInformation($"Письмо успешно отправлено: {subject}");
        }
        catch (SmtpException e)
        {
            _logger.LogError(e.Message, e.StatusCode);
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var smtpClient = new SmtpClient
        {
            Host = Settings.SmtpHost,
            Port = Settings.SmtpPort,
            EnableSsl = Settings.EnableSsl
        };

        if (Settings.SmtpAuthenticationEnabled)
        {
            try
            {
                if (!string.IsNullOrEmpty(Settings.SmtpPassword))
                {
                    smtpClient.Credentials = new NetworkCredential(
                        Settings.SmtpUser, _cryptService.Unprotect(Settings.SmtpPassword));
                }
                else
                {
                    _logger.LogError("SmtpPassword is empty, credentials cannot create");
                }
            }
            catch (CryptographicException)
            {
                Settings.IsEnable = false;
                _logger.LogError("Cannot decrypt password, mail notification disable");
            }
        }
        else
        {
            smtpClient.UseDefaultCredentials = false;
        }

        smtpClient.SendCompleted += SendCompletedCallback;
        return smtpClient;
    }
}*/