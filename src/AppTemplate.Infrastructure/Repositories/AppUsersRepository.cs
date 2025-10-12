using AppTemplate.Application.Data.DynamicQuery;
using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Infrastructure.Repositories;

public sealed class AppUsersRepository
    : Repository<AppUser, Guid>, IAppUsersRepository
{
  public AppUsersRepository(ApplicationDbContext dbContext) : base(dbContext) { }

  public async Task<Result<AppUser>> GetUserByIdWithIdentityAndRrolesAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    var user = await GetAsync(
        predicate: u => u.Id == userId,
        include: query => query
            .Include(u => u.IdentityUser)
            .Include(u => u.Roles),
        asNoTracking: true,
        cancellationToken: cancellationToken);

    if (user is null)
      return Result.NotFound("User not found.");

    return Result.Success(user);
  }

  public async Task<Result<AppUser>> GetUserByIdentityIdWithIdentityAndRolesAsync(string identityId, CancellationToken cancellationToken = default)
  {
    var user = await GetAsync(
        predicate: u => u.IdentityId == identityId,
        include: query => query
            .Include(u => u.IdentityUser)
            .Include(u => u.Roles),
        asNoTracking: true,
        cancellationToken: cancellationToken);

    if (user is null)
      return Result.NotFound("User not found.");

    return Result.Success(user);
  }

  public async Task<Result<PaginatedList<AppUser>>> GetAllUsersWithIdentityAndRolesAsync(
      int pageIndex,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    var users = await GetAllAsync(
        pageIndex: pageIndex,
        pageSize: pageSize,
        includeSoftDeleted: false,
        include: query => query
            .Include(u => u.IdentityUser)
            .Include(u => u.Roles.Where(r => r.DeletedOnUtc == null)),
        cancellationToken: cancellationToken);

    return Result.Success(users);
  }

  public async Task<Result<PaginatedList<AppUser>>> GetAllUsersByRoleIdWithIdentityAndRolesAsync(
      Guid roleId,
      int pageIndex,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    var users = await GetAllAsync(
        pageIndex: pageIndex,
        pageSize: pageSize,
        predicate: user => user.Roles.Any(role => role.Id == roleId),
        includeSoftDeleted: false,
        include: query => query
            .Include(u => u.IdentityUser)
            .Include(u => u.Roles),
        cancellationToken: cancellationToken);

    return Result.Success(users);
  }

  public async Task<Result<PaginatedList<AppUser>>> GetAllUsersDynamicWithIdentityAndRolesAsync(
      DynamicQuery dynamicQuery,
      int pageIndex,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    var users = await GetAllDynamicAsync(
        dynamicQuery: dynamicQuery,
        pageIndex: pageIndex,
        pageSize: pageSize,
        includeSoftDeleted: false,
        include: query => query
            .Include(u => u.IdentityUser)
            .Include(u => u.Roles.Where(r => r.DeletedOnUtc == null)),
        cancellationToken: cancellationToken);

    return Result.Success(users);
  }

  public async Task<Result<AppUser>> GetUserWithRolesAndIdentityByIdAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    var user = await GetAsync(
        predicate: u => u.Id == userId,
        include: query => query
            .Include(u => u.Roles)
            .Include(u => u.IdentityUser),
        asNoTracking: false,
        cancellationToken: cancellationToken);

    if (user is null)
      return Result.NotFound("User not found.");

    return Result.Success(user);
  }

  public async Task<Result<AppUser>> GetUserWithRolesAndIdentityByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default)
  {
    var user = await GetAsync(
        predicate: u => u.IdentityId == identityId,
        include: query => query
            .Include(u => u.Roles)
            .Include(u => u.IdentityUser),
        asNoTracking: false,
        cancellationToken: cancellationToken);

    if (user is null)
      return Result.NotFound("User not found.");

    return Result.Success(user);
  }

  public async Task<int> GetUsersCountAsync(bool includeSoftDeleted = false, CancellationToken cancellationToken = default)
  {
    return await CountAsync(null, includeSoftDeleted, cancellationToken);
  }

  public async Task AddAsync(AppUser entity, CancellationToken cancellationToken = default)
  {
    await base.AddAsync(entity, cancellationToken);
  }

  public void Update(AppUser entity, CancellationToken cancellationToken = default)
  {
    base.Update(entity);
  }

  public void Delete(AppUser entity, bool isSoftDelete = true, CancellationToken cancellationToken = default)
  {
    base.Delete(entity, isSoftDelete);
  }
}