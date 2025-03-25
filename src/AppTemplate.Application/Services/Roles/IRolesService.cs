using AppTemplate.Domain.Roles;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using System.Linq.Expressions;

namespace AppTemplate.Application.Services.Roles;

public interface IRolesService
{
    Task<Role> GetAsync(
        Expression<Func<Role, bool>> predicate,
        bool includeSoftDeleted = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<Role, object>>[] include);

    Task<PaginatedList<Role>> GetAllAsync(
        int index = 0,
        int size = 10,
        bool includeSoftDeleted = false,
        Expression<Func<Role, bool>>? predicate = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<Role, object>>[] include);

    Task AddAsync(Role role);
    void Update(Role role);
    void Delete(Role role);
}
