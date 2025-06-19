using Myrtus.Clarity.Core.Application.Abstractions.Caching;

namespace AppTemplate.Application.Features.Statistics.Roles.Queries.GetRoleStatistics;

public sealed record GetRoleStatisticsQuery() : ICachedQuery<GetRoleStatisticsResponse>
{
    public string CacheKey => "roles-statistics";
    
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}