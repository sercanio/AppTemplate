using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;

namespace AppTemplate.Infrastructure.Repositories;

internal sealed class AppUsersRepository(ApplicationDbContext dbContext) : Repository<AppUser>(dbContext), IAppUsersRepository
{
    public async Task<AppUser?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetAsync(user => user.Id == id, cancellationToken: cancellationToken);
    }

    public override async Task AddAsync(AppUser user)
    {
        foreach (Role role in user.Roles)
        {
            DbContext.Attach(role);
        }

        await DbContext.AddAsync(user);
    }
}
