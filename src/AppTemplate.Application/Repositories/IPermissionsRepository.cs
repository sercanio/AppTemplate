using AppTemplate.Domain.Roles;

namespace AppTemplate.Application.Repositories;

public interface IPermissionsRepository : IRepository<Permission, Guid>
{
}
