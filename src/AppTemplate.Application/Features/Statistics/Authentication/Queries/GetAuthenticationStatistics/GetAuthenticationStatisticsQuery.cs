using AppTemplate.Application.Services.Caching;

namespace AppTemplate.Application.Features.Statistics.Authentication.Queries.GetAuthenticationStatistics;

public sealed record GetAuthenticationStatisticsQuery() : ICachedQuery<GetAuthenticationStatisticsResponse>
{
    public string CacheKey => "authentication-statistics";
    
    public TimeSpan? Expiration => TimeSpan.FromMinutes(1);
}