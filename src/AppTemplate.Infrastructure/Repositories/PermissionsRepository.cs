using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles;

namespace AppTemplate.Infrastructure.Repositories;

internal sealed class PermissionsRepository(ApplicationDbContext dbContext) 
    : Repository<Permission>(dbContext), IPermissionsRepository
{
}