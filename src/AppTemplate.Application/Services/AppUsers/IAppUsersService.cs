using AppTemplate.Domain.AppUsers;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using System.Linq.Expressions;

namespace AppTemplate.Application.Services.AppUsers;

public interface IAppUsersService
{
    Task<AppUser> GetAsync(
        Expression<Func<AppUser, bool>> predicate,
        bool includeSoftDeleted = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<AppUser, object>>[] include);

    Task<AppUser> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PaginatedList<AppUser>> GetAllAsync(
        int index = 0,
        int size = 10,
        bool includeSoftDeleted = false,
        Expression<Func<AppUser, bool>>? predicate = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<AppUser, object>>[] include);

    Task AddAsync(AppUser user);

    void Update(AppUser user);

    void Delete(AppUser user);
}
