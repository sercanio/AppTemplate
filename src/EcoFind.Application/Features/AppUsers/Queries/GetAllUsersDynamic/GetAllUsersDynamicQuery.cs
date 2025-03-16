using Ardalis.Result;
using MediatR;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Dynamic;

namespace EcoFind.Application.Features.AppUsers.Queries.GetAllUsersDynamic;

public record GetAllUsersDynamicQuery(
        int PageIndex,
        int PageSize,
        DynamicQuery DynamicQuery) : IRequest<Result<IPaginatedList<GetAllUsersDynamicQueryResponse>>>;
