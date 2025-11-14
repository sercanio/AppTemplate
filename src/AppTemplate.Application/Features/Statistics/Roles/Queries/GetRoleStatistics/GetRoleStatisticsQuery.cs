using AppTemplate.Application.Services.Caching;

namespace AppTemplate.Application.Features.Statistics.Roles.Queries.GetRoleStatistics;

public sealed record GetRoleStatisticsQuery() : ICachedQuery<GetRoleStatisticsQueryResponse>
{
  public string CacheKey => "roles-statistics";

  public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}