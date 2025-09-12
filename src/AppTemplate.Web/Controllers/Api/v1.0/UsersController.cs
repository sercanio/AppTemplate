using AppTemplate.Application.Authorization;
using AppTemplate.Application.Data.DynamicQuery;
using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Enums;
using AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;
using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsers;
using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;
using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersDynamic;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Features.AppUsers.Queries.GetUser;
using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Web.Attributes;
using AppTemplate.Web.Controllers.Api;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AppTemplate.Web.Controllers;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("Fixed")]
public class UsersController : BaseController
{
    public UsersController(ISender sender, IErrorHandlingService errorHandlingService)
        : base(sender, errorHandlingService)
    {
    }

    [HttpGet]
    [HasPermission(Permissions.UsersRead)]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        GetAllUsersQuery query = new(pageIndex, pageSize);

        Result<PaginatedList<GetAllUsersQueryResponse>> result = await _sender.Send(query, cancellationToken);

        return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok(result.Value);
    }

    [HttpPost("dynamic")]
    [HasPermission(Permissions.UsersRead)]
    public async Task<IActionResult> GetAllUsersDynamic(
        [FromBody] DynamicQuery dynamicQuery,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
    {
        GetAllUsersDynamicQuery query = new(
            pageIndex,
            pageSize,
            dynamicQuery
        );

        Result<PaginatedList<GetAllUsersDynamicQueryResponse>> result = await _sender.Send(query, cancellationToken);

        return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok(result.Value);
    }

    [HttpGet("{userId}")]
    [HasPermission(Permissions.UsersRead)]
    public async Task<IActionResult> GetUserById(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        GetUserQuery query = new(userId);

        Result<GetUserQueryResponse> result = await _sender.Send(query, cancellationToken);

        return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok(result.Value);
    }

    [HttpGet("roles/{roleId}")]
    [HasPermission(Permissions.UsersRead)]
    public async Task<IActionResult> GetAllUsersByRoleId(
            Guid roleId,
            [FromQuery] int PageIndex = 0,
            [FromQuery] int PageSize = 10,
            CancellationToken cancellationToken = default)
    {
        GetAllUsersByRoleIdQuery query = new(PageIndex, PageSize, roleId);

        Result<PaginatedList<GetAllUsersByRoleIdQueryResponse>> result = await _sender.Send(query, cancellationToken);
        return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok(result.Value);
    }

    [HttpPatch("{userId}/roles")]
    [HasPermission(Permissions.UsersUpdate)]
    public async Task<IActionResult> UpdateUserRoles(
        UpdateUserRolesRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        UpdateUserRolesCommand command = new(userId, request.Operation, request.RoleId);

        Result<UpdateUserRolesCommandResponse> result = await _sender.Send(command, cancellationToken);

        return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok(result.Value);
    }
}

public sealed record UpdateUserRolesRequest(Operation Operation, Guid RoleId);
