using AppTemplate.Application.Authorization;
using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Features.Permissions.Queries.GetAllPermissions;
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
public class PermissionsController : BaseController
{
    public PermissionsController(ISender sender, IErrorHandlingService errorHandlingService)
        : base(sender, errorHandlingService)
    {
    }

    [HttpGet]
    [HasPermission(Permissions.PermissionsRead)]
    public async Task<IActionResult> GetAllPermissions(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllPermissionsQuery(pageIndex, pageSize);
        Result<PaginatedList<GetAllPermissionsQueryResponse>> result = await _sender.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return _errorHandlingService.HandleErrorResponse(result);
        }
        return Ok(result.Value);
    }
}
