using Myrtus.Clarity.Core.Application.Repositories;
using EcoFind.Domain.AppUsers;

namespace EcoFind.Application.Repositories;

public interface IAppUsersRepository : IRepository<AppUser>
{
    Task<AppUser?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
