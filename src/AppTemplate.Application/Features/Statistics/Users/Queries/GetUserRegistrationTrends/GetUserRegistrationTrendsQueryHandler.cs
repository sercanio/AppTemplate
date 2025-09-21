using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using MediatR;

namespace AppTemplate.Application.Features.Statistics.Users.Queries.GetUserRegistrationTrends;

public sealed class GetUserRegistrationTrendsQueryHandler : IRequestHandler<GetUserRegistrationTrendsQuery, Result<GetUserRegistrationTrendsQueryResponse>>
{
    private readonly IAppUsersRepository _userRepository;

    public GetUserRegistrationTrendsQueryHandler(IAppUsersRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<GetUserRegistrationTrendsQueryResponse>> Handle(
        GetUserRegistrationTrendsQuery request, 
        CancellationToken cancellationToken)
    {
        var result = await _userRepository.GetAllUsersWithIdentityAndRolesAsync(
            pageIndex: 0,
            pageSize: int.MaxValue,
            cancellationToken: cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return Result.Error("Could not retrieve users.");
        }

        var allUsers = result.Value.Items;
        var today = DateTime.UtcNow.Date;

        var usersThisMonth = GetUsersThisMonth(allUsers, today);
        var usersLastMonth = GetUsersLastMonth(allUsers, today);

        int totalUsersLastMonth = usersLastMonth.Count;
        int totalUsersThisMonth = usersThisMonth.Count;
        int growthPercentage = CalculateGrowthPercentage(totalUsersLastMonth, totalUsersThisMonth);

        var last30Days = GetLast30Days(today);
        var dailyRegistrations = GetDailyRegistrations(allUsers, last30Days);

        var response = new GetUserRegistrationTrendsQueryResponse(
            TotalUsersLastMonth: totalUsersLastMonth,
            TotalUsersThisMonth: totalUsersThisMonth,
            GrowthPercentage: growthPercentage,
            DailyRegistrations: dailyRegistrations
        );

        return Result.Success(response);
    }

    private static List<AppUser> GetUsersThisMonth(IEnumerable<AppUser> users, DateTime today)
    {
        var firstDayOfThisMonth = new DateTime(today.Year, today.Month, 1);
        return users.Where(u => u.CreatedOnUtc >= firstDayOfThisMonth).ToList();
    }

    private static List<AppUser> GetUsersLastMonth(IEnumerable<AppUser> users, DateTime today)
    {
        var firstDayOfThisMonth = new DateTime(today.Year, today.Month, 1);
        var firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);
        return users.Where(u => u.CreatedOnUtc >= firstDayOfLastMonth && u.CreatedOnUtc < firstDayOfThisMonth).ToList();
    }

    private static List<DateTime> GetLast30Days(DateTime today)
    {
        return Enumerable.Range(0, 30)
            .Select(i => today.AddDays(-i))
            .ToList();
    }

    private static Dictionary<string, int> GetDailyRegistrations(IEnumerable<AppUser> users, List<DateTime> last30Days)
    {
        var dailyRegistrations = new Dictionary<string, int>();
        foreach (var day in last30Days)
        {
            string dateKey = day.ToString("MM-dd");
            int count = users.Count(u => u.CreatedOnUtc.Date == day);
            dailyRegistrations.Add(dateKey, count);
        }
        return dailyRegistrations;
    }

    private static int CalculateGrowthPercentage(int lastMonth, int thisMonth)
    {
        return lastMonth > 0
            ? (int)Math.Round((double)(thisMonth - lastMonth) / lastMonth * 100)
            : 100;
    }
}