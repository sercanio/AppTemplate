using AppTemplate.Application.Repositories;
using AppTemplate.Core.Infrastructure.Pagination;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using MediatR;
using System.Data;

namespace AppTemplate.Application.Features.Roles.Queries.GetAllRoles;

public sealed class GetAllRolesQueryHandler(IRolesRepository roleRepository) : IRequestHandler<GetAllRolesQuery, Result<PaginatedList<GetAllRolesQueryResponse>>>
{
  private readonly IRolesRepository _roleRepository = roleRepository;

  public async Task<Result<PaginatedList<GetAllRolesQueryResponse>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
  {
    PaginatedList<Role> roles = await _roleRepository.GetAllAsync(
         pageIndex: request.PageIndex,
         pageSize: request.PageSize,
         cancellationToken: cancellationToken);

    List<GetAllRolesQueryResponse> mappedRoles = roles.Items.Select(role =>
        new GetAllRolesQueryResponse(role.Id.ToString(), role.Name.Value, role.DisplayName.Value, role.IsDefault)).ToList();

    PaginatedList<GetAllRolesQueryResponse> paginatedList = new(
        mappedRoles,
        roles.TotalCount,
        request.PageIndex,
        request.PageSize
    );

    return Result.Success<PaginatedList<GetAllRolesQueryResponse>>(paginatedList);
  }
}
