using Ardalis.Result;
using MediatR;
using Myrtus.Clarity.Core.Infrastructure.Dynamic;
using Myrtus.Clarity.Core.Infrastructure.Pagination;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersDynamic;

public record GetAllUsersDynamicQuery(
        int PageIndex,
        int PageSize,
        DynamicQuery DynamicQuery) : IRequest<Result<PaginatedList<GetAllUsersDynamicQueryResponse>>>;

