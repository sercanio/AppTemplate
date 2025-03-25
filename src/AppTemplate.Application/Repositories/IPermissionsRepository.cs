using AppTemplate.Domain.Roles;
using Myrtus.Clarity.Core.Application.Repositories;

namespace AppTemplate.Application.Repositories;

public interface IPermissionsRepository : IRepository<Permission>
{
}
