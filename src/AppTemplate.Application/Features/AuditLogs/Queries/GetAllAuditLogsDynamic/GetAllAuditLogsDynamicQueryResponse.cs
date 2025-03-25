namespace AppTemplate.Application.Features.AuditLogs.Queries.GetAllAuditLogsDynamic;
public sealed record GetAllAuditLogsDynamicQueryResponse(
        Guid Id,
        string User,
        string Action,
        string Entity,
        string EntityId,
        DateTime Timestamp,
        string Details);
