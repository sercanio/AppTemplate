using Myrtus.Clarity.Core.Domain.Abstractions;

namespace AppTemplate.Domain.Notifications;

public class Notification : Entity
{
    public Notification()
    {
        // Default constructor for EF Core
    }

    public Notification(Guid userId, string user, string action, string entity, string entityId, string details)
    {
        UserId = userId;
        User = user;
        Action = action;
        Entity = entity;
        EntityId = entityId;
        Details = details;
        Timestamp = DateTime.UtcNow;
        IsRead = false;
    }

    public Guid UserId { get; set; } = Guid.Empty;
    public string User { get; set; } = "";
    public string Action { get; set; } = "";
    public string Entity { get; set; } = "";
    public string EntityId { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Details { get; set; } = "";
    public bool IsRead { get; set; } = false;
    public Dictionary<string, object>? AdditionalData { get; set; }
}
