using AppTemplate.Domain.AppUsers;

namespace AppTemplate.Application.Repositories;

public interface IAppUsersRepository : IRepository<AppUser, Guid>
{
}