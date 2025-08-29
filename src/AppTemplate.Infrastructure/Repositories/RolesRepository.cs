using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles;

namespace AppTemplate.Infrastructure.Repositories;

internal sealed class RolesRepository(ApplicationDbContext dbContext)
        : Repository<Role, Guid>(dbContext), IRolesRepository
{
}