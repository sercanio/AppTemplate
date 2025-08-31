using AppTemplate.Application.Repositories;
using AppTemplate.Core.Infrastructure.Pagination;
using AppTemplate.Domain.Roles;
using Ardalis.Result;

namespace AppTemplate.Infrastructure.Repositories;

public sealed class PermissionsRepository
    : Repository<Permission, Guid>, IPermissionsRepository
{
    public PermissionsRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<Result<PaginatedList<Permission>>> GetAllPermissionsAsync(
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var permissions = await GetAllAsync(
            pageIndex: pageIndex,
            pageSize: pageSize,
            includeSoftDeleted: false,
            asNoTracking: true,
            cancellationToken: cancellationToken);

        return Result.Success(permissions);
    }
}