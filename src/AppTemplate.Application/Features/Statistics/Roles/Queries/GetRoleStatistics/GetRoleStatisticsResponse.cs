namespace AppTemplate.Application.Features.Statistics.Roles.Queries.GetRoleStatistics;

public sealed record GetRoleStatisticsResponse(
    int TotalRoles,
    int TotalPermissions,
    Dictionary<string, int> PermissionsPerRole,
    Dictionary<string, int> UsersPerRole,
    Dictionary<string, int> PermissionsByFeature);