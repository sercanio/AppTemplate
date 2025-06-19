using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Application.Features.Statistics.Roles.Queries.GetRoleStatistics;

public sealed class GetRoleStatisticsQueryHandler : IRequestHandler<GetRoleStatisticsQuery, Result<GetRoleStatisticsResponse>>
{
    private readonly IRolesRepository _rolesRepository;
    private readonly IPermissionsRepository _permissionsRepository;

    public GetRoleStatisticsQueryHandler(
        IRolesRepository rolesRepository,
        IPermissionsRepository permissionsRepository)
    {
        _rolesRepository = rolesRepository;
        _permissionsRepository = permissionsRepository;
    }

    public async Task<Result<GetRoleStatisticsResponse>> Handle(
        GetRoleStatisticsQuery request, 
        CancellationToken cancellationToken)
    {
        // Get all roles with their permissions and users
        var roles = await _rolesRepository.GetAllAsync(
            pageIndex:0,
            pageSize: int.MaxValue, 
            include: [
                r => r.Permissions,
                r => r.Users
            ],
            cancellationToken: cancellationToken);

        // Get all permissions
        var permissions = await _permissionsRepository.GetAllAsync(
            pageIndex:0,
            pageSize: int.MaxValue,
            cancellationToken: cancellationToken);

        // Calculate statistics
        int totalRoles = roles.Items.Count;
        int totalPermissions = permissions.Items.Count;

        // Calculate permissions per role
        var permissionsPerRole = new Dictionary<string, int>();
        foreach (var role in roles.Items)
        {
            permissionsPerRole[role.Name.Value] = role.Permissions.Count;
        }

        // Calculate users per role
        var usersPerRole = new Dictionary<string, int>();
        foreach (var role in roles.Items)
        {
            usersPerRole[role.Name.Value] = role.Users.Count;
        }

        // Calculate permissions by feature
        var permissionsByFeature = permissions.Items
            .GroupBy(p => p.Feature)
            .ToDictionary(g => g.Key, g => g.Count());

        // Create response
        var response = new GetRoleStatisticsResponse(
            TotalRoles: totalRoles,
            TotalPermissions: totalPermissions,
            PermissionsPerRole: permissionsPerRole,
            UsersPerRole: usersPerRole,
            PermissionsByFeature: permissionsByFeature
        );

        return Result.Success(response);
    }
}