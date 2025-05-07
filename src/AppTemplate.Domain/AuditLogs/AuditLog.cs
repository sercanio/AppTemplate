using AppTemplate.Domain.AppUsers;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Newtonsoft.Json;

namespace AppTemplate.Domain.AuditLogs;

public class AuditLog : Entity
{
    private const string SystemUser = "System";

    public AuditLog()
    {
        // Default constructor for EF Core
    }

    private AuditLog(string action, string entity, string entityId, string details)
        : this(SystemUser, action, entity, entityId, details, null)
    {
    }

    private AuditLog(string user, string action, string entity, string entityId, string details, string? userId, Guid? appUserId = null)
    {
        UserId = userId;
        User = string.IsNullOrWhiteSpace(user) ? SystemUser : user;
        Action = action;
        Entity = entity;
        EntityId = entityId;
        Details = details;
        Timestamp = DateTime.UtcNow;
        if (appUserId.HasValue)
        {
            AppUserId = appUserId.Value;
        }
    }

    public string? UserId { get; set; }
    public string User { get; set; } = SystemUser;
    public string Action { get; set; } = "";
    public string Entity { get; set; } = "";
    public string EntityId { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Details { get; set; } = "";
    public Dictionary<string, object>? AdditionalData { get; set; }

    public Guid? AppUserId { get; private set; }

    [JsonIgnore]
    public AppUser? AppUser { get; private set; }

    public static AuditLog CreateSystemLog(string action, string entity, string entityId, string details)
    {
        return new AuditLog(SystemUser, action, entity, entityId, details, null);
    }

    public static AuditLog CreateUserLog(string user, string action, string entity, string entityId, string details, string userId, Guid appUserId)
    {
        return new AuditLog(user, action, entity, entityId, details, userId, appUserId);
    }
}
