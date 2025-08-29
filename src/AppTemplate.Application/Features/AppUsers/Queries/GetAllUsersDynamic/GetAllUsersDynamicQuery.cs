using Ardalis.Result;
using MediatR;
using AppTemplate.Core.Infrastructure.Pagination;
using AppTemplate.Core.Infrastructure.DynamicQuery;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersDynamic;

public record GetAllUsersDynamicQuery(
        int PageIndex,
        int PageSize,
        DynamicQuery DynamicQuery) : IRequest<Result<PaginatedList<GetAllUsersDynamicQueryResponse>>>;

