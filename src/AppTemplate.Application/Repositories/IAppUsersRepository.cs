using AppTemplate.Domain.AppUsers;
using Myrtus.Clarity.Core.Application.Abstractions.Repositories;

namespace AppTemplate.Application.Repositories;

public interface IAppUsersRepository : IRepository<AppUser, Guid>
{
}