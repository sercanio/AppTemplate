using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using MediatR;

namespace AppTemplate.Application.Features.Statistics.Users.Queries.GetUserRegistrationTrends;

public sealed class GetUserRegistrationTrendsQueryHandler : IRequestHandler<GetUserRegistrationTrendsQuery, Result<GetUserRegistrationTrendsResponse>>
{
    private readonly IAppUsersRepository _userRepository;

    public GetUserRegistrationTrendsQueryHandler(IAppUsersRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<GetUserRegistrationTrendsResponse>> Handle(
        GetUserRegistrationTrendsQuery request, 
        CancellationToken cancellationToken)
    {
        // Get all users - we'll filter in memory to avoid complex DB queries
        var allUsers = await _userRepository.GetAllAsync(
            pageIndex: 0,
            pageSize: int.MaxValue, // Get all users
            includeSoftDeleted: false,
            cancellationToken: cancellationToken);

        // Define time periods
        var today = DateTime.UtcNow.Date;
        var firstDayOfThisMonth = new DateTime(today.Year, today.Month, 1);
        var firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);
        var lastDayOfLastMonth = firstDayOfThisMonth.AddDays(-1);
        
        // Filter users by creation date
        var usersThisMonth = allUsers.Items
            .Where(u => u.CreatedOnUtc >= firstDayOfThisMonth)
            .ToList();
        
        var usersLastMonth = allUsers.Items
            .Where(u => u.CreatedOnUtc >= firstDayOfLastMonth && u.CreatedOnUtc < firstDayOfThisMonth)
            .ToList();
        
        // Calculate growth percentage
        int totalUsersLastMonth = usersLastMonth.Count;
        int totalUsersThisMonth = usersThisMonth.Count;
        int growthPercentage = totalUsersLastMonth > 0 
            ? (int)Math.Round((double)(totalUsersThisMonth - totalUsersLastMonth) / totalUsersLastMonth * 100) 
            : 100;
        
        // Calculate daily registrations for the last 30 days
        var last30Days = Enumerable.Range(0, 30)
            .Select(i => today.AddDays(-i))
            .ToList();
        
        var dailyRegistrations = new Dictionary<string, int>();
        
        foreach (var day in last30Days)
        {
            string dateKey = day.ToString("MM-dd");
            int count = allUsers.Items.Count(u => u.CreatedOnUtc.Date == day);
            dailyRegistrations.Add(dateKey, count);
        }

        // Create response
        var response = new GetUserRegistrationTrendsResponse(
            TotalUsersLastMonth: totalUsersLastMonth,
            TotalUsersThisMonth: totalUsersThisMonth,
            GrowthPercentage: growthPercentage,
            DailyRegistrations: dailyRegistrations
        );

        return Result.Success(response);
    }
}