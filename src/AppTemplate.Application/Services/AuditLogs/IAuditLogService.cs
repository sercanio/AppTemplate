using AppTemplate.Domain.AuditLogs;

namespace AppTemplate.Application.Services.AuditLogs;

public interface IAuditLogService
{
    Task LogAsync(AuditLog log);
}