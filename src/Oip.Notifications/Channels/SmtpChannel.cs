using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using Oip.Base.Services;
using Oip.Notifications.Base;

namespace Oip.Notifications.Channels;

/// <summary>
/// A channel that sends notifications via SMTP email using configured mail settings and encryption services
/// </summary>
public class SmtpChannel : INotificationChannel
{
    private readonly ILogger<SmtpChannel> _logger;
    private readonly CryptService _cryptService;
    private readonly SmtpSettings _settings;

    /// <inheritdoc />
    public string Code { get; set; } = typeof(SmtpChannel).FullName!;

    /// <inheritdoc />
    public string Name { get; set; } = "Smtp client";

    /// <inheritdoc />
    public int MaxRetryCount { get; set; } = 5;

    /// <summary>A channel that sends notifications via SMTP email using configured mail settings and encryption services</summary>
    public SmtpChannel(ILogger<SmtpChannel> logger, CryptService cryptService, IConfiguration configuration)
    {
        ServicePointManager.SecurityProtocol |= (SecurityProtocolType)0xC00;
        _logger = logger;
        _cryptService = cryptService;
        _settings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>() ?? new SmtpSettings();

        // Settings validation
        if (string.IsNullOrEmpty(_settings.MailFrom))
            throw new ArgumentException("MailFrom is required in SmtpSettings");

        if (string.IsNullOrEmpty(_settings.SmtpHost))
            throw new ArgumentException("SmtpHost is required in SmtpSettings");
    }

    private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
    {
        _logger.LogInformation("Message sent");
        ((SmtpClient)sender).Dispose();
    }

    /// <inheritdoc />
    public bool IsEnable { get; set; } = true;

    /// <inheritdoc />
    public bool RequiresVerification { get; set; } = false;

    /// <inheritdoc />
    public void OpenChannel()
    {
        // Nothing do
    }

    /// <inheritdoc />
    public void CloseChannel()
    {
        // Nothing do
    }

    /// <inheritdoc />
    public void Notify(UserInfoDto userInfoDto, string subject, string message)
    {
        Notify(userInfoDto, subject, message, Array.Empty<Attachment>());
    }

    /// <inheritdoc />
    public void Notify(UserInfoDto userInfoDto, string subject, string message, Attachment[]? attachments)
    {
        var smtpClient = CreateSmtpClient();
        var mail = new MailMessage(_settings.MailFrom, userInfoDto.Email)
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
            _logger.LogInformation("Email successfully sent: {Subject}", subject);
        }
        catch (SmtpException e)
        {
            _logger.LogError("SMTP error when sending email {Subject}: {StatusCode} {Message}", subject, e.StatusCode,
                e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("General error when sending email {Subject}: {Message}", subject, e.Message);
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var smtpClient = new SmtpClient
        {
            Host = _settings.SmtpHost,
            Port = _settings.SmtpPort,
            EnableSsl = _settings.EnableSsl
        };

        if (_settings.SmtpAuthenticationEnabled)
        {
            try
            {
                if (!string.IsNullOrEmpty(_settings.SmtpPassword))
                {
                    var decryptedPassword = _cryptService.Unprotect(_settings.SmtpPassword);
                    if (string.IsNullOrEmpty(decryptedPassword))
                    {
                        _logger.LogError("Failed to decrypt SMTP password");
                        throw new InvalidOperationException("Failed to decrypt SMTP password");
                    }

                    smtpClient.Credentials = new NetworkCredential(_settings.SmtpUser, decryptedPassword);
                }
                else
                {
                    _logger.LogError("SmtpPassword is empty, credentials cannot create");
                }
            }
            catch (CryptographicException)
            {
                _settings.IsEnable = false;
                _logger.LogError("Cannot decrypt password, email notification disabled");
            }
        }
        else
        {
            smtpClient.UseDefaultCredentials = false;
        }

        smtpClient.SendCompleted += SendCompletedCallback;
        return smtpClient;
    }
}

internal class SmtpSettings
{
    public string MailFrom { get; set; } = string.Empty;
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public bool SmtpAuthenticationEnabled { get; set; } = true;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool IsEnable { get; set; } = true;
}