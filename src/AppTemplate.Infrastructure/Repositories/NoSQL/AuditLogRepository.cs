using System.Linq.Expressions;
using MongoDB.Driver;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using AppTemplate.Application.Repositories.NoSQL;

namespace AppTemplate.Infrastructure.Repositories.NoSQL;

internal sealed class AuditLogRepository : NoSqlRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(IMongoDatabase database)
        : base(database, "AuditLogs")
    {
    }
    public async Task<IPaginatedList<AuditLog>> GetAllAuditLogsAsync(
        Expression<Func<AuditLog, bool>>? predicate = null,
        int pageIndex = 0,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await GetAllAsync<object>(
            predicate: predicate,
            orderBy: x => x.Timestamp,
            descending: true,
            pageIndex: pageIndex,
            pageSize: pageSize,
            cancellationToken: cancellationToken);
    }
}
