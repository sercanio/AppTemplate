using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
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
        Expression<Func<AppUser, bool>>? predicate = null,
        bool includeSoftDeleted = false,
        Func<IQueryable<AppUser>, IQueryable<AppUser>>? include = null,
        CancellationToken cancellationToken = default)
    {
        PaginatedList<AppUser> users = await _userRepository.GetAllAsync(
            pageIndex: index,
            pageSize: size,
            predicate: predicate,
            includeSoftDeleted: includeSoftDeleted,
            include: include,
            cancellationToken: cancellationToken);

        return new PaginatedList<AppUser>(
            users.Items,
            users.TotalCount,
            users.PageIndex,
            users.PageSize);
    }

    public async Task<AppUser?> GetAsync(
        Expression<Func<AppUser, bool>> predicate,
        bool includeSoftDeleted = false,
        Func<IQueryable<AppUser>, IQueryable<AppUser>>? include = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetAsync(
            predicate: predicate,
            include: include,
            asNoTracking: asNoTracking,
            includeSoftDeleted: includeSoftDeleted,
            cancellationToken: cancellationToken);
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

    public async Task<int> GetUsersCountAsync(bool includeSoftDeleted = false, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetUsersCountAsync(includeSoftDeleted, cancellationToken);
    }

    public async Task<Result<AppUser>> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(identityId))
        {
            return Result.Error(AppUserErrors.IdentityIdNotFound.Name);
        }

        var user = await _userRepository.GetAsync(
            predicate: u => u.IdentityId == identityId,
            includeSoftDeleted: false,
            asNoTracking: false,
            cancellationToken: cancellationToken);

        return user != null 
            ? Result.Success(user) 
            : Result.Error(AppUserErrors.NotFound.Name);
    }
}
