using Myrtus.Clarity.Core.Application.Abstractions.Caching;

namespace AppTemplate.Application.Features.Statistics.Users.Queries.GetUserRegistrationTrends;

public sealed record GetUserRegistrationTrendsQuery() : ICachedQuery<GetUserRegistrationTrendsResponse>
{
    public string CacheKey => "users-registration-trends";
    
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}