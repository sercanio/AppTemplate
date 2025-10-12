using AppTemplate.Application.Data.Pagination;
using AppTemplate.Domain.Roles;
using Ardalis.Result;

namespace AppTemplate.Application.Repositories;

public interface IPermissionsRepository : IRepository<Permission, Guid>
{
    Task<Result<PaginatedList<Permission>>> GetAllPermissionsAsync(
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);
}
