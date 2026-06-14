using Oip.Data.Repositories;
using Oip.Notifications.Base.Data.Contexts;
using Oip.Notifications.Base.Data.Entities;

namespace Oip.Notifications.Base.Data.Repositories;

/// <summary>
/// Repository for managing user notification records.
/// </summary>
/// <param name="context">The database context</param>
public class NotificationUserRepository(NotificationsDbContext context)
    : BaseRepository<NotificationUserEntity, long>(context);
