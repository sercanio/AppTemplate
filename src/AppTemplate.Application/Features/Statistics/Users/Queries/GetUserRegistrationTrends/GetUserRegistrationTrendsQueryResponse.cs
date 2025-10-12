namespace AppTemplate.Application.Features.Statistics.Users.Queries.GetUserRegistrationTrends;

public sealed record GetUserRegistrationTrendsQueryResponse(
    int TotalUsersLastMonth,
    int TotalUsersThisMonth,
    int GrowthPercentage,
    Dictionary<string, int> DailyRegistrations);