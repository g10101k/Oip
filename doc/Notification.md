Я хочу написать нотификацию в aspnet(efcore)+angular(primeng) проработай мне слой хранения данных - Entity.

NotificationType - отдельная сущность которая описывает тип нотификации, у нее есть категория имя  
NotificationTemplate - это сущность которая описывает кому нужно отправить нотификации, условия нотификации, шаблон сообщения, канал передачи'
````csharp
// Entities/Notification.cs

namespace Oip.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTimeOffset CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTimeOffset? ReadDate { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string Category { get; set; }
        public string ActionUrl { get; set; }
        public int? RelatedEntityId { get; set; }
        public string RelatedEntityType { get; set; }
    }

    public enum NotificationType
    {
       
    }
}

````
