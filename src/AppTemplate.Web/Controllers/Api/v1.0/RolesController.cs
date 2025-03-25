using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Authorization;
using Myrtus.Clarity.Core.WebAPI;
using Myrtus.Clarity.Core.WebAPI.Controllers;
using AppTemplate.Application.Enums;
using AppTemplate.Application.Features.Roles.Commands.Create;
using AppTemplate.Application.Features.Roles.Commands.Delete;
using AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;
using AppTemplate.Application.Features.Roles.Commands.Update.UpdateRoleName;
using AppTemplate.Application.Features.Roles.Queries.GetAllRoles;
using AppTemplate.Application.Features.Roles.Queries.GetRoleById;
using ApiVersion = Microsoft.AspNetCore.Mvc.ApiVersion;
using AppTemplate.Web.Controllers.Api;

namespace AppTemplate.Web.Controllers;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("Fixed")]
public class RolesController : BaseController
{
    public RolesController(ISender sender, IErrorHandlingService errorHandlingService)
        : base(sender, errorHandlingService)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllRolesQuery(pageIndex, pageSize);
        Result<IPaginatedList<GetAllRolesQueryResponse>> result = await _sender.Send(query, cancellationToken);

        return !result.IsSuccess
            ? _errorHandlingService.HandleErrorResponse(result)
            : Ok(result.Value);
    }

    [HttpGet("{roleId}")]
    //[HasPermission(Permissions.RolesRead)]
    public async Task<IActionResult> GetRoleById([FromRoute] Guid roleId, CancellationToken cancellationToken = default)
    {
        GetRoleByIdQuery query = new(roleId);
        Result<GetRoleByIdQueryResponse> result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpPost]
    //[HasPermission(Permissions.RolesCreate)]
    public async Task<IActionResult> CreateRole(
            CreateRoleRequest request,
            CancellationToken cancellationToken = default)
    {
        CreateRoleCommand command = new(request.Name);
        Result<CreateRoleCommandResponse> result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpPatch("{roleId}/permissions")]
    //[HasPermission(Permissions.RolesUpdate)]
    public async Task<IActionResult> UpdateRolePermissions(
            [FromBody] UpdateRolePermissionsRequest request,
            [FromRoute] Guid roleId,
            CancellationToken cancellationToken = default)
    {
        UpdateRolePermissionsCommand command = new(roleId, request.PermissionId, request.Operation);
        Result<UpdateRolePermissionsCommandResponse> result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpPatch("{roleId}/name")]
    //[HasPermission(Permissions.RolesUpdate)]
    public async Task<IActionResult> UpdateRoleName(
            [FromBody] UpdateRoleNameRequest request,
            [FromRoute] Guid roleId,
            CancellationToken cancellationToken = default)
    {
        UpdateRoleNameCommand command = new(roleId, request.Name);
        Result<UpdateRoleNameCommandResponse> result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpDelete("{roleId}")]
    //[HasPermission(Permissions.RolesDelete)]
    public async Task<IActionResult> DeleteRole(
            [FromRoute] Guid roleId,
            CancellationToken cancellationToken = default)
    {
        DeleteRoleCommand command = new(roleId);
        Result<DeleteRoleCommandResponse> result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : _errorHandlingService.HandleErrorResponse(result);
    }

    public sealed record CreateRoleRequest(string Name);
    public sealed record UpdateRolePermissionsRequest(Guid PermissionId, Operation Operation);
    public sealed record UpdateRoleNameRequest(string Name);
}
