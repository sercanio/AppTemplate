using Myrtus.Clarity.Core.Application.Repositories;
using AppTemplate.Domain.AppUsers;

namespace AppTemplate.Application.Repositories;

public interface IAppUsersRepository : IRepository<AppUser>
{
    Task<AppUser?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
