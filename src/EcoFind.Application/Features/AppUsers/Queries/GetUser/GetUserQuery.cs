using Myrtus.Clarity.Core.Application.Abstractions.Caching;

namespace EcoFind.Application.Features.AppUsers.Queries.GetUser;

public sealed record GetUserQuery(
   Guid UserId) : ICachedQuery<GetUserQueryResponse>
{
    public string CacheKey => $"users-{UserId}";

    public TimeSpan? Expiration => null;
}
