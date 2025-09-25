using AppTemplate.Application.Data.Pagination;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using System.Linq.Expressions;

namespace AppTemplate.Application.Services.AppUsers;

public interface IAppUsersService
{
    Task<AppUser?> GetAsync(
        Expression<Func<AppUser, bool>> predicate,
        bool includeSoftDeleted = false,
        Func<IQueryable<AppUser>, IQueryable<AppUser>>? include = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default);

    Task<AppUser> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PaginatedList<AppUser>> GetAllAsync(
        int index = 0,
        int size = 10,
        Expression<Func<AppUser, bool>>? predicate = null,
        bool includeSoftDeleted = false,
        Func<IQueryable<AppUser>, IQueryable<AppUser>>? include = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(AppUser user);

    void Update(AppUser user);

    void Delete(AppUser user);

    Task<int> GetUsersCountAsync(bool includeSoftDeleted = false, CancellationToken cancellationToken = default);

    Task<Result<AppUser>> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default);
}
