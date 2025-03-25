namespace AppTemplate.Application.Features.AuditLogs.Queries.GetAllAuditLogs;
public sealed record GetAllAuditLogsQueryResponse(
    Guid Id,
    string User,
    string Action,
    string Entity,
    string EntityId,
    DateTime Timestamp,
    string Details);

