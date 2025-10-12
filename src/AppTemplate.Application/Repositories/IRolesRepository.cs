using AppTemplate.Application.Data.Pagination;
using AppTemplate.Domain.Roles;
using Ardalis.Result;

namespace AppTemplate.Application.Repositories;

public interface IRolesRepository : IRepository<Role, Guid>
{
    Task<Result<Role>> GetRoleByIdWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);

    Task<Result<PaginatedList<Role>>> GetAllRolesAsync(
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);
}