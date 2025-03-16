using System.Data;
using MediatR;
using Ardalis.Result;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using EcoFind.Application.Repositories;
using EcoFind.Domain.Roles;

namespace EcoFind.Application.Features.Roles.Queries.GetAllRoles;

public sealed class GetAllRolesQueryHandler(IRolesRepository roleRepository) : IRequestHandler<GetAllRolesQuery, Result<IPaginatedList<GetAllRolesQueryResponse>>>
{
    private readonly IRolesRepository _roleRepository = roleRepository;

    public async Task<Result<IPaginatedList<GetAllRolesQueryResponse>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        IPaginatedList<Role> roles = await _roleRepository.GetAllAsync(
            pageIndex: request.PageIndex,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        List<GetAllRolesQueryResponse> mappedRoles = roles.Items.Select(role =>
            new GetAllRolesQueryResponse(role.Id.ToString(), role.Name.Value, role.IsDefault.Value)).ToList();

        PaginatedList<GetAllRolesQueryResponse> paginatedList = new(
            mappedRoles,
            roles.TotalCount,
            request.PageIndex,
            request.PageSize
        );

        return Result.Success<IPaginatedList<GetAllRolesQueryResponse>>(paginatedList);
    }
}
