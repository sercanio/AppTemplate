using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AuditLogs;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Infrastructure.Repositories;

internal sealed class AuditLogsRepository(ApplicationDbContext dbContext) : Repository<AuditLog>(dbContext), IAuditLogsRepository
{
    public async Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(
        string entity,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        // This query will use the composite index
        return await DbContext.AuditLogs
            .AsNoTracking()
            .Where(a => a.Entity == entity && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> SearchAuditLogsInJsonDataAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default)
    {
        // Uses the GIN index on AdditionalData
        return await DbContext.AuditLogs
            .AsNoTracking()
            .Where(a => EF.Functions.JsonContains(a.AdditionalData, $"{{\"{key}\": \"{value}\"}}"))
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }
}
