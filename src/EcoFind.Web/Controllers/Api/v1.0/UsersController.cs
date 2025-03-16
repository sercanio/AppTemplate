using Ardalis.Result;
using EcoFind.Application.Enums;
using EcoFind.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;
using EcoFind.Application.Features.AppUsers.Queries.GetAllUsersDynamic;
using EcoFind.Application.Features.AppUsers.Queries.GetLoggedInUser;
using EcoFind.Application.Features.AppUsers.Queries.GetUser;
using EcoFind.Application.Features.Users.Queries.GetAllUsers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Authorization;
using Myrtus.Clarity.Core.Infrastructure.Dynamic;
using Myrtus.Clarity.Core.WebAPI;
using Myrtus.Clarity.Core.WebAPI.Controllers;
using EcoFind.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;

namespace EcoFind.Web.Controllers;

[EnableRateLimiting("Fixed")]
[Route("api/v1.0/[controller]")]
[ApiController]
public class UsersController : BaseController
{
    public UsersController(ISender sender, IErrorHandlingService errorHandlingService)
        : base(sender, errorHandlingService)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        GetAllUsersQuery query = new(pageIndex, pageSize);

        Result<IPaginatedList<GetAllUsersQueryResponse>> result = await _sender.Send(query, cancellationToken);

        return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok(result.Value);
    }

    [HttpPost("dynamic")]
    //[HasPermission(Permissions.UsersRead)]
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

        Result<IPaginatedList<GetAllUsersDynamicQueryResponse>> result = await _sender.Send(query, cancellationToken);

        return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok(result.Value);
    }

    [HttpGet("{userId}")]
    //[HasPermission(Permissions.UsersRead)]
    public async Task<IActionResult> GetUserById(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        GetUserQuery query = new(userId);

        Result<GetUserQueryResponse> result = await _sender.Send(query, cancellationToken);

        return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok(result.Value);
    }

    [HttpGet("roles/{roleId}")]
    // [HasPermission(Permissions.UsersRead)]
    public async Task<IActionResult> GetAllUsersByRoleId(
            Guid roleId,
            [FromQuery] int PageIndex = 0,
            [FromQuery] int PageSize = 10,
            CancellationToken cancellationToken = default)
    {
        GetAllUsersByRoleIdQuery query = new(PageIndex, PageSize, roleId);

        Result<IPaginatedList<GetAllUsersByRoleIdQueryResponse>> result = await _sender.Send(query, cancellationToken);
        return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok(result.Value);
    }

    [HttpPatch("{userId}/roles")]
    //[HasPermission(Permissions.UsersRead)]
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
