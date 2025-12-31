using System.Net.Mail;

namespace Oip.Notifications.Base;

/// <summary>
/// Defines a channel for sending notifications
/// </summary>
public interface INotificationChannel
{
    /// <summary>
    /// Имя канала
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Активность канала
    /// </summary>
    bool IsEnable { get; }

    /// <summary>
    /// Открытие канала
    /// </summary>
    void OpenChannel();

    /// <summary>
    /// Закрытие канала
    /// </summary>
    void CloseChannel();

    /// <summary>
    /// Оповещение
    /// </summary>
    void Send(UserInfoDto userInfoDto, string subject, string message);

    /// <summary>
    /// Оповещение с вложением
    /// </summary>
    /// <param name="userInfoDto"></param>
    /// <param name="subject"></param>
    /// <param name="message"></param>
    /// <param name="attachments"></param>
    void Send(UserInfoDto userInfoDto, string subject, string message, Attachment[] attachments);
}

public class UserInfoDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}