using AppTemplate.Application.Services.Statistics;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Application.Features.Statistics.Authentication.Queries.GetAuthenticationStatistics;

public sealed class GetAuthenticationStatisticsQueryHandler : IRequestHandler<GetAuthenticationStatisticsQuery, Result<GetAuthenticationStatisticsResponse>>
{
    private readonly IActiveSessionService _sessionService;
    private readonly UserManager<IdentityUser> _userManager;

    public GetAuthenticationStatisticsQueryHandler(
        IActiveSessionService sessionService,
        UserManager<IdentityUser> userManager)
    {
        _sessionService = sessionService;
        _userManager = userManager;
    }

    public async Task<Result<GetAuthenticationStatisticsResponse>> Handle(
        GetAuthenticationStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        // Get active sessions count
        int activeSessions = await _sessionService.GetActiveSessionsCountAsync();
        
        // Get 2FA statistics
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        int twoFactorEnabled = users.Count(u => u.TwoFactorEnabled);
        
        // Count users with authenticator setup (this requires retrieving each user's authenticator key)
        int usersWithAuthenticator = 0;
        foreach (var user in users)
        {
            string authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (!string.IsNullOrEmpty(authenticatorKey))
            {
                usersWithAuthenticator++;
            }
        }
        
        // For login successes/failures, you'd need to implement tracking for these events
        // Here we're using placeholder values
        int successfulLogins = 0; // Replace with actual tracking
        int failedLogins = 0;     // Replace with actual tracking
        
        var response = new GetAuthenticationStatisticsResponse(
            ActiveSessions: activeSessions,
            SuccessfulLogins: successfulLogins,
            FailedLogins: failedLogins,
            TwoFactorEnabled: twoFactorEnabled,
            TotalUsersWithAuthenticator: usersWithAuthenticator
        );
        
        return Result.Success(response);
    }
}