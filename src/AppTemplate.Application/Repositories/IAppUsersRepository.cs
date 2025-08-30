using AppTemplate.Core.Infrastructure.Pagination;
using AppTemplate.Core.Infrastructure.DynamicQuery;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;

namespace AppTemplate.Application.Repositories;

public interface IAppUsersRepository : IRepository<AppUser, Guid>
{
  Task<Result<AppUser>> GetUserByIdWithIdentityAndRrolesAsync(Guid userId, CancellationToken cancellationToken = default);

  Task<Result<AppUser>> GetUserByIdentityIdWithIdentityAndRolesAsync(string identityId, CancellationToken cancellationToken = default);

  Task<Result<PaginatedList<AppUser>>> GetAllUsersWithIdentityAndRolesAsync(
      int pageIndex,
      int pageSize,
      CancellationToken cancellationToken = default);

  Task<Result<PaginatedList<AppUser>>> GetAllUsersByRoleIdWithIdentityAndRolesAsync(
      Guid roleId,
      int pageIndex,
      int pageSize,
      CancellationToken cancellationToken = default);

  Task<Result<PaginatedList<AppUser>>> GetAllUsersDynamicWithIdentityAndRolesAsync(
      DynamicQuery dynamicQuery,
      int pageIndex,
      int pageSize,
      CancellationToken cancellationToken = default);

  Task<Result<AppUser>> GetUserWithRolesAndIdentityByIdAsync(Guid userId, CancellationToken cancellationToken = default);
  Task<Result<AppUser>> GetUserWithRolesAndIdentityByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default);
  Task<int> GetUsersCountAsync(bool includeSoftDeleted = false, CancellationToken cancellationToken = default);
}