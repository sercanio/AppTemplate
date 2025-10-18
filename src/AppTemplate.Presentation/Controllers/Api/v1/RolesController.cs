using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Enums;
using AppTemplate.Application.Features.Roles.Commands.Create;
using AppTemplate.Application.Features.Roles.Commands.Delete;
using AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;
using AppTemplate.Application.Features.Roles.Commands.Update.UpdateRoleName;
using AppTemplate.Application.Features.Roles.Queries.GetAllRoles;
using AppTemplate.Application.Features.Roles.Queries.GetRoleById;
using AppTemplate.Application.Services.Authorization;
using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Presentation.Attributes;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ApiVersion = Microsoft.AspNetCore.Mvc.ApiVersion;

namespace AppTemplate.Presentation.Controllers.Api.v1;

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
  [HasPermission(Permissions.RolesRead)]
  public async Task<IActionResult> GetAllRoles(
      [FromQuery] int pageIndex = 0,
      [FromQuery] int pageSize = 10,
      CancellationToken cancellationToken = default)
  {
    var query = new GetAllRolesQuery(pageIndex, pageSize);
    Result<PaginatedList<GetAllRolesQueryResponse>> result = await _sender.Send(query, cancellationToken);

    return !result.IsSuccess
        ? _errorHandlingService.HandleErrorResponse(result)
        : Ok(result.Value);
  }

  [HttpGet("{roleId}")]
  [HasPermission(Permissions.RolesRead)]
  public async Task<IActionResult> GetRoleById([FromRoute] Guid roleId, CancellationToken cancellationToken = default)
  {
    GetRoleByIdQuery query = new(roleId);
    Result<GetRoleByIdQueryResponse> result = await _sender.Send(query, cancellationToken);

    return result.IsSuccess ? Ok(result.Value) : _errorHandlingService.HandleErrorResponse(result);
  }

  [HttpPost]
  [HasPermission(Permissions.RolesCreate)]
  public async Task<IActionResult> CreateRole(
          CreateRoleRequest request,
          CancellationToken cancellationToken = default)
  {
    CreateRoleCommand command = new(request.Name, request.DisplayName);
    Result<CreateRoleCommandResponse> result = await _sender.Send(command, cancellationToken);

    return result.IsSuccess ? Ok(result.Value) : _errorHandlingService.HandleErrorResponse(result);
  }

  [HttpPatch("{roleId}/permissions")]
  [HasPermission(Permissions.RolesUpdate)]
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
  [HasPermission(Permissions.RolesUpdate)]
  public async Task<IActionResult> UpdateRoleName(
          [FromBody] UpdateRoleNameRequest request,
          [FromRoute] Guid roleId,
          CancellationToken cancellationToken = default)
  {
    UpdateRoleNameCommand command = new(roleId, request.Name, request.DisplayName);
    Result<UpdateRoleNameCommandResponse> result = await _sender.Send(command, cancellationToken);

    return result.IsSuccess ? Ok(result.Value) : _errorHandlingService.HandleErrorResponse(result);
  }

  [HttpDelete("{roleId}")]
  [HasPermission(Permissions.RolesDelete)]
  public async Task<IActionResult> DeleteRole(
          [FromRoute] Guid roleId,
          CancellationToken cancellationToken = default)
  {
    DeleteRoleCommand command = new(roleId);
    Result<DeleteRoleCommandResponse> result = await _sender.Send(command, cancellationToken);

    return result.IsSuccess ? Ok(result.Value) : _errorHandlingService.HandleErrorResponse(result);
  }

  public sealed record CreateRoleRequest(string Name, string DisplayName);
  public sealed record UpdateRolePermissionsRequest(Guid PermissionId, Operation Operation);
  public sealed record UpdateRoleNameRequest(string Name, string DisplayName);
}
