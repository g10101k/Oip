namespace Oip.Notifications.Data.Enums;

/// <summary>
/// Delivery status values
/// </summary>
public enum DeliveryStatus
{
    /// <summary>Delivery is pending</summary>
    Pending = 0,

    /// <summary>Delivery is being processed</summary>
    Processing = 1,

    /// <summary>Notification has been sent</summary>
    Sent = 2,

    /// <summary>Notification has been delivered</summary>
    Delivered = 3,

    /// <summary>Delivery failed</summary>
    Failed = 4,

    /// <summary>Delivery was cancelled</summary>
    Cancelled = 5
}