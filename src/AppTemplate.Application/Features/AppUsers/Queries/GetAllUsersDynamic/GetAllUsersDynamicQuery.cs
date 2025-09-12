using AppTemplate.Application.Data.DynamicQuery;
using AppTemplate.Application.Data.Pagination;
using Ardalis.Result;
using MediatR;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersDynamic;

public record GetAllUsersDynamicQuery(
        int PageIndex,
        int PageSize,
        DynamicQuery DynamicQuery) : IRequest<Result<PaginatedList<GetAllUsersDynamicQueryResponse>>>;

