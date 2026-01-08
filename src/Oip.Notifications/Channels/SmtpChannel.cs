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
// ReSharper disable once UnusedType.Global
public class SmtpChannel : INotificationChannel
{
    private readonly ILogger<SmtpChannel> _logger;
    private readonly CryptService _cryptService;
    private readonly SmtpSettings _settings;
    private readonly CancellationToken _stoppingToken = CancellationToken.None;
    private bool _isProcessing;

    /// <inheritdoc />
    public Queue<NotificationDto> Queue { get; set; } = new();

    /// <inheritdoc />
    public string Code { get; set; } = typeof(SmtpChannel).FullName!;

    /// <inheritdoc />
    public string Name { get; set; } = "Smtp client";

    /// <inheritdoc />
    public int MaxRetryCount { get; set; } = 5;

    /// <summary>
    /// Пробует получить первый элемент очереди и осуществить обработку
    /// </summary>
    private Action ProcessQueueAction => async void () =>
    {
        try
        {
            while (Queue.TryPeek(out var message))
            {
                try
                {
                    await ProcessQueueInternal(message, _stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Ошибка в ProcessQueueAction");
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "При попытке получения сообщения из очереди");
        }
        finally
        {
            _isProcessing = false;
        }
    };

    private async Task ProcessQueueInternal(NotificationDto message, CancellationToken stoppingToken)
    {
        var retry = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogTrace("Обработка запроса {Subject} попытка №{Retry}", message.Subject, retry);

                Notify(message);

                _ = Queue.Dequeue();
                return;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex) when (retry < _settings.RetryCount)
            {
                var delay = _settings.ProcessingIntervalMs * retry;
                _logger.LogError(ex,
                    "Ошибка обработки запроса {Subject}, попытка №{Retry} - следующая попытка через {Delay}ms \r\n",
                    message.Subject, retry, delay);
                await Task.Delay(delay, _stoppingToken);
                retry++;
            }
            catch (Exception ex) when (retry == _settings.RetryCount)
            {
                _logger.LogError(ex,
                    "Не удалось отправить запрос {Subject}, сообщение удалено будет удалено из очереди",
                    message.Subject);
                _ = Queue.Dequeue();
                return;
            }
        }
    }

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

    public void Notify(NotificationDto notification)
    {
        var smtpClient = CreateSmtpClient();
        var mail = new MailMessage(_settings.MailFrom, notification.User.Email)
        {
            Subject = notification.Subject,
            Body = notification.Message,
        };
        foreach (var attachment in notification.Attachment)
        {
            mail.Attachments.Add(attachment);
        }

        try
        {
            smtpClient.Send(mail);
            _logger.LogInformation("Email successfully sent: {Subject}", notification.Subject);
        }
        catch (SmtpException e)
        {
            _logger.LogError("SMTP error when sending email {Subject}: {StatusCode} {Message}", notification.Subject,
                e.StatusCode,
                e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("General error when sending email {Subject}: {Message}", notification.Subject, e.Message);
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

    /// <summary>
    /// Обработка запроса
    /// </summary>
    public void ProcessNotify(NotificationDto message, CancellationToken cancellationToken = default)
    {
        Queue.Enqueue(message);

        if (_isProcessing) return;

        _isProcessing = true;
        _ = Task.Run(ProcessQueueAction, cancellationToken);
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

    /// <summary>
    /// Брать из настроек канала
    /// </summary>
    public int RetryCount { get; set; } = 5;

    public int ProcessingIntervalMs { get; set; } = 100;
}