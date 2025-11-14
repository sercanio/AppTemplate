using System.Linq.Expressions;
using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles;
using Ardalis.Result;

namespace AppTemplate.Application.Services.Roles;

public sealed class RolesService(IRolesRepository roleRepository) : IRolesService
{
  private readonly IRolesRepository _roleRepository = roleRepository;

  public async Task AddAsync(Role role)
  {
    await _roleRepository.AddAsync(role);
  }

  public void Delete(Role role)
  {
    _roleRepository.Delete(role);
  }

  public void Update(Role role)
  {
    _roleRepository.Update(role);
  }

  public async Task<Role> GetAsync(
      Expression<Func<Role, bool>> predicate,
      bool includeSoftDeleted = false,
      Func<IQueryable<Role>, IQueryable<Role>>? include = null,
      bool asNoTracking = true,
      CancellationToken cancellationToken = default)
  {
    Role? role = await _roleRepository.GetAsync(
        predicate: predicate,
        includeSoftDeleted: includeSoftDeleted,
        include: include,
        asNoTracking: asNoTracking,
        cancellationToken: cancellationToken);

    return role!;
  }

  public async Task<PaginatedList<Role>> GetAllAsync(
      int index = 0,
      int size = 10,
      Expression<Func<Role, bool>>? predicate = null,
      bool includeSoftDeleted = false,
      Func<IQueryable<Role>, IQueryable<Role>>? include = null,
      bool asNoTracking = true,
      CancellationToken cancellationToken = default)
  {
    PaginatedList<Role> roles = await _roleRepository.GetAllAsync(
        pageIndex: index,
        pageSize: size,
        predicate: predicate,
        includeSoftDeleted: includeSoftDeleted,
        include: include,
        asNoTracking: asNoTracking,
        cancellationToken: cancellationToken);

    PaginatedList<Role> paginatedList = new(
        roles.Items,
        roles.TotalCount,
        roles.PageIndex,
        roles.PageSize);

    return paginatedList;
  }

  public async Task<Result<Role>> GetDefaultRole(CancellationToken cancellationToken = default)
  {
    var role = await _roleRepository.GetAsync(
        r => r.IsDefault,
        asNoTracking: false,
        cancellationToken: cancellationToken);

    if (role != null)
      return Result.Success(role);

    return Result.Error("Default role not found");
  }
}
