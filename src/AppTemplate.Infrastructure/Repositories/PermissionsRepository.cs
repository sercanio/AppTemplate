using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles;

namespace AppTemplate.Infrastructure.Repositories;

internal sealed class PermissionsRepository
    : Repository<Permission, Guid>, IPermissionsRepository
{
  public PermissionsRepository(ApplicationDbContext dbContext) : base(dbContext) { }
}