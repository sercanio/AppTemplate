using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;

namespace AppTemplate.Infrastructure.Repositories;

internal sealed class AppUsersRepository
    : Repository<AppUser, Guid>, IAppUsersRepository
{
  public AppUsersRepository(ApplicationDbContext dbContext) : base(dbContext) { }

}