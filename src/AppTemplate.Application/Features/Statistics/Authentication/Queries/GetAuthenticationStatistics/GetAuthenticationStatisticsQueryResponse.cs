namespace AppTemplate.Application.Features.Statistics.Authentication.Queries.GetAuthenticationStatistics;

public sealed record GetAuthenticationStatisticsQueryResponse(
    int ActiveSessions,
    int SuccessfulLogins,
    int FailedLogins,
    int TwoFactorEnabled,
    int TotalUsersWithAuthenticator);