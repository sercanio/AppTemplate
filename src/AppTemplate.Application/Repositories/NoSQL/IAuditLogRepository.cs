using System.Linq.Expressions;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Domain.Abstractions;

namespace AppTemplate.Application.Repositories.NoSQL;

public interface IAuditLogRepository : INoSqlRepository<AuditLog>
{
    Task<IPaginatedList<AuditLog>> GetAllAuditLogsAsync(
        Expression<Func<AuditLog, bool>>? predicate = null,
        int pageIndex = 0,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
