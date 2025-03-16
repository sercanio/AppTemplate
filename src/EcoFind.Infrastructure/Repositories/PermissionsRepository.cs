using EcoFind.Application.Repositories;
using EcoFind.Domain.Roles;

namespace EcoFind.Infrastructure.Repositories;

internal sealed class PermissionsRepository(ApplicationDbContext dbContext) 
    : Repository<Permission>(dbContext), IPermissionsRepository
{
}