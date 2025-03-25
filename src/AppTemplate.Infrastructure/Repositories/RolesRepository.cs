using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles;

namespace AppTemplate.Infrastructure.Repositories;

internal sealed class RolesRepository(ApplicationDbContext dbContext) : Repository<Role>(dbContext), IRolesRepository
{
    public override async Task AddAsync(Role role)
    {
        await DbContext.AddAsync(role);
    }

    public override void Update(Role role)
    {
        DbContext.Update(role);
    }
}
