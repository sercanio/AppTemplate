using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Infrastructure.Repositories;

public sealed class RolesRepository : Repository<Role, Guid>, IRolesRepository
{
  public RolesRepository(ApplicationDbContext dbContext) : base(dbContext) { }

  public async Task<Result<Role>> GetRoleByIdWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
  {
    var role = await GetAsync(
        predicate: r => r.Id == roleId,
        include: query => query.Include(r => r.Permissions),
        asNoTracking: false,
        cancellationToken: cancellationToken);

    if (role is null)
      return Result.NotFound("Role not found.");

    return Result.Success(role);
  }

  public async Task<Result<PaginatedList<Role>>> GetAllRolesAsync(
      int pageIndex,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    var roles = await GetAllAsync(
        pageIndex: pageIndex,
        pageSize: pageSize,
        includeSoftDeleted: false,
        asNoTracking: true,
        cancellationToken: cancellationToken);

    return Result.Success(roles);
  }
}
