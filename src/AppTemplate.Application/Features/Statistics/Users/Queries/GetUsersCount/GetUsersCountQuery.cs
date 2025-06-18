using Myrtus.Clarity.Core.Application.Abstractions.Caching;

namespace AppTemplate.Application.Features.Statistics.Users.Queries.GetUsersCount;

public sealed record GetUsersCountQuery() : ICachedQuery<GetUsersCountQueryResponse>
{
    public string CacheKey => "users-Count";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}
