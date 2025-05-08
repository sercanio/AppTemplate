using AppTemplate.Application.Repositories;
using Ardalis.Result;
using MediatR;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Pagination;

namespace AppTemplate.Application.Features.AuditLogs.Queries.GetAllAuditLogs;

public sealed class GetAllAuditLogsQueryHandler(IAuditLogsRepository auditLogRepository)
           : IRequestHandler<GetAllAuditLogsQuery, Result<IPaginatedList<GetAllAuditLogsQueryResponse>>>
{
    public async Task<Result<IPaginatedList<GetAllAuditLogsQueryResponse>>> Handle(GetAllAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = auditLogRepository.GetAllAsync(
            pageIndex: request.PageIndex,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken
        );

        var auditLogs = await query;

        var paginatedAuditLogs = auditLogs.Items
            .OrderByDescending(x => x.Timestamp)
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
