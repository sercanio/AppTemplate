using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using System.Linq.Expressions;

namespace AppTemplate.Application.Services.AppUsers;

public class AppUsersService(IAppUsersRepository userRepository) : IAppUsersService
{
    private readonly IAppUsersRepository _userRepository = userRepository;

    public async Task AddAsync(AppUser user)
    {
        await _userRepository.AddAsync(user);
    }

    public void Delete(AppUser user)
    {
        _userRepository.Delete(user);
    }

    public void Update(AppUser user)
    {
        _userRepository.Update(user);
    }

    public async Task<PaginatedList<AppUser>> GetAllAsync(
        int index = 0,
        int size = 10,
        bool includeSoftDeleted = false,
        Expression<Func<AppUser, bool>>? predicate = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<AppUser, object>>[] include)
    {
        IPaginatedList<AppUser> users = await _userRepository.GetAllAsync(
            index,
            size,
            includeSoftDeleted,
            predicate,
            cancellationToken,
            include);

        PaginatedList<AppUser> paginatedList = new(
            users.Items,
            users.TotalCount,
            users.PageIndex,
            users.PageSize);

        return paginatedList;
    }

    public async Task<AppUser> GetAsync(Expression<Func<AppUser, bool>> predicate, bool includeSoftDeleted = false, CancellationToken cancellationToken = default, params Expression<Func<AppUser, object>>[] include)
    {
        var role = await _userRepository.GetAsync(
            predicate,
            includeSoftDeleted,
            cancellationToken,
            include);

        return role!;
    }

    public async Task<AppUser> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(
            predicate: user => user.Id == id,
            cancellationToken: cancellationToken);
        return user!;
    }
}
