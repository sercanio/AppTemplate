using AppTemplate.Domain.Roles;
using Myrtus.Clarity.Core.Application.Abstractions.Repositories;

namespace AppTemplate.Application.Repositories;

public interface IPermissionsRepository : IRepository<Permission, Guid>
{
}
