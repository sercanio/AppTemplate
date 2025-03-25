using Ardalis.Result;
using MediatR;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using AppTemplate.Application.Repositories.NoSQL;

namespace AppTemplate.Application.Features.AuditLogs.Queries.GetAllAuditLogs;

public sealed class GetAllAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
            : IRequestHandler<GetAllAuditLogsQuery, Result<IPaginatedList<GetAllAuditLogsQueryResponse>>>
{
    public async Task<Result<IPaginatedList<GetAllAuditLogsQueryResponse>>> Handle(GetAllAuditLogsQuery request, CancellationToken cancellationToken)
    {
        IPaginatedList<AuditLog> auditLogs = await auditLogRepository.GetAllAuditLogsAsync(
            pageIndex: request.PageIndex,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        var paginatedAuditLogs = auditLogs.Items
            .Select(auditLog => new GetAllAuditLogsQueryResponse(
                auditLog.Id,
                auditLog.User,
                auditLog.Action,
                auditLog.Entity,
                auditLog.EntityId,
                auditLog.Timestamp,
                auditLog.Details
            ))
            .ToList();

        return Result.Success<IPaginatedList<GetAllAuditLogsQueryResponse>>(
            new PaginatedList<GetAllAuditLogsQueryResponse>(
                paginatedAuditLogs,
                auditLogs.TotalCount,
                auditLogs.PageIndex,
                auditLogs.PageSize
            ));
    }
}
