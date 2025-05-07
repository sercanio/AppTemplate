using AppTemplate.Domain.AuditLogs;
using Myrtus.Clarity.Core.Application.Repositories;

namespace AppTemplate.Application.Repositories;

public interface IAuditLogsRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(
        string entity, 
        string entityId, 
        CancellationToken cancellationToken = default);
        
    Task<IEnumerable<AuditLog>> SearchAuditLogsInJsonDataAsync(
        string key, 
        string value, 
        CancellationToken cancellationToken = default);
}
