using AppTemplate.Application.Services.Caching;

namespace AppTemplate.Application.Features.Statistics.Users.Queries.GetUsersCount;

public sealed record GetUsersCountQuery() : ICachedQuery<GetUsersCountQueryResponse>
{
  public string CacheKey => "users-count";

  public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}
