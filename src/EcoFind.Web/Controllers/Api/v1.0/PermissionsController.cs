using Ardalis.Result;
using EcoFind.Application.Features.Permissions.Queries.GetAllPermissions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.WebAPI;
using Myrtus.Clarity.Core.WebAPI.Controllers;

namespace EcoFind.Web.Controllers;

[EnableRateLimiting("Fixed")]
[Route("api/v1.0/[controller]")]
[ApiController]
public class PermissionsController : BaseController
{
    public PermissionsController(ISender sender, IErrorHandlingService errorHandlingService)
        : base(sender, errorHandlingService)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPermissions(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllPermissionsQuery(pageIndex, pageSize);
        Result<IPaginatedList<GetAllPermissionsQueryResponse>> result = await _sender.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return _errorHandlingService.HandleErrorResponse(result);
        }
        return Ok(result.Value);
    }
}
