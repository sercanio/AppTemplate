using MediatR;
using Ardalis.Result;
using AppTemplate.Application.Repositories;
using AppTemplate.Core.Infrastructure.Pagination;

namespace AppTemplate.Application.Features.Roles.Queries.GetAllRoles;

public sealed class GetAllRolesQueryHandler(IRolesRepository roleRepository) : IRequestHandler<GetAllRolesQuery, Result<PaginatedList<GetAllRolesQueryResponse>>>
{
    private readonly IRolesRepository _roleRepository = roleRepository;

    public async Task<Result<PaginatedList<GetAllRolesQueryResponse>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var result = await _roleRepository.GetAllRolesAsync(
            request.PageIndex,
            request.PageSize,
            cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return Result.Error("Could not retrieve roles.");
        }

        var roles = result.Value;

        List<GetAllRolesQueryResponse> mappedRoles = roles.Items.Select(role =>
            new GetAllRolesQueryResponse(role.Id.ToString(), role.Name.Value, role.DisplayName.Value, role.IsDefault)).ToList();

        PaginatedList<GetAllRolesQueryResponse> paginatedList = new(
            mappedRoles,
            roles.TotalCount,
            request.PageIndex,
            request.PageSize
        );

        return Result.Success(paginatedList);
    }
}
