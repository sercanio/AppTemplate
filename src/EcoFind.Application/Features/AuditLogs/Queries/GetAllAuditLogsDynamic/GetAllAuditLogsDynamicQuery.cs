using Ardalis.Result;
using MediatR;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Dynamic;

namespace EcoFind.Application.Features.AuditLogs.Queries.GetAllAuditLogsDynamic;

public sealed record GetAllAuditLogsDynamicQuery(
        int PageIndex,
        int PageSize,
        DynamicQuery DynamicQuery) : IRequest<Result<IPaginatedList<GetAllAuditLogsDynamicQueryResponse>>>;
