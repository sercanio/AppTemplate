using AppTemplate.Application.Services.Caching;

namespace AppTemplate.Application.Features.Statistics.Users.Queries.GetUserRegistrationTrends;

public sealed record GetUserRegistrationTrendsQuery() : ICachedQuery<GetUserRegistrationTrendsQueryResponse>
{
    public string CacheKey => "users-registration-trends";
    
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}