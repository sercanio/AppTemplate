using AppTemplate.Core.Infrastructure.Pagination;
using AppTemplate.Domain.Roles;
using System.Linq.Expressions;

namespace AppTemplate.Application.Services.Roles;

public interface IRolesService
{
  Task<Role> GetAsync(
      Expression<Func<Role, bool>> predicate,
      bool includeSoftDeleted = false,
      Func<IQueryable<Role>, IQueryable<Role>>? include = null,
      bool asNoTracking = true,
      CancellationToken cancellationToken = default);

  Task<PaginatedList<Role>> GetAllAsync(
      int index = 0,
      int size = 10,
      Expression<Func<Role, bool>>? predicate = null,
      bool includeSoftDeleted = false,
      Func<IQueryable<Role>, IQueryable<Role>>? include = null,
      bool asNoTracking = true,
      CancellationToken cancellationToken = default);

  Task AddAsync(Role role);
  void Update(Role role);
  void Delete(Role role);
}
