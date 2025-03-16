using Ardalis.Result;
using MediatR;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Myrtus.Clarity.Core.Infrastructure.Dynamic;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using EcoFind.Application.Repositories.NoSQL;

namespace EcoFind.Application.Features.AuditLogs.Queries.GetAllAuditLogsDynamic;

public class GetAllAuditLogsDynamicQueryHandler : IRequestHandler<GetAllAuditLogsDynamicQuery, Result<IPaginatedList<GetAllAuditLogsDynamicQueryResponse>>>
{
    private readonly INoSqlRepository<AuditLog> _auditLogRepository;

    public GetAllAuditLogsDynamicQueryHandler(INoSqlRepository<AuditLog> auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<Result<IPaginatedList<GetAllAuditLogsDynamicQueryResponse>>> Handle(
        GetAllAuditLogsDynamicQuery request,
        CancellationToken cancellationToken)
    {
        IPaginatedList<AuditLog> auditLogs = await _auditLogRepository.GetAllAsync(
                        pageIndex: 0,
                        pageSize: int.MaxValue,
                        predicate: null,
                        cancellationToken: cancellationToken);

        IQueryable<AuditLog> filteredAuditLogs = auditLogs.Items.AsQueryable().ToDynamic(request.DynamicQuery);

        List<GetAllAuditLogsDynamicQueryResponse> paginatedAuditLogs = filteredAuditLogs
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .Select(auditLog => new GetAllAuditLogsDynamicQueryResponse(
                auditLog.Id,
                auditLog.User,
                auditLog.Action,
                auditLog.Entity,
                auditLog.EntityId,
                auditLog.Timestamp,
                auditLog.Details
            )).ToList();

        PaginatedList<GetAllAuditLogsDynamicQueryResponse> paginatedList = new(
            paginatedAuditLogs,
            filteredAuditLogs.Count(),
            request.PageIndex,
            request.PageSize
        );

        return Result.Success<IPaginatedList<GetAllAuditLogsDynamicQueryResponse>>(paginatedList);
    }
}
