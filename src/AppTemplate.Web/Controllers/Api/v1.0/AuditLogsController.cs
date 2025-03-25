using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Authorization;
using Myrtus.Clarity.Core.Infrastructure.Dynamic;
using Myrtus.Clarity.Core.WebAPI;
using Myrtus.Clarity.Core.WebAPI.Controllers;
using AppTemplate.Application.Features.AuditLogs.Queries.GetAllAuditLogs;
using AppTemplate.Application.Features.AuditLogs.Queries.GetAllAuditLogsDynamic;
using Microsoft.AspNetCore.RateLimiting;
using AppTemplate.Web.Controllers.Api;

namespace AppTemplate.Web.Controllers;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("Fixed")]
public class AuditLogsController(ISender sender, IErrorHandlingService errorHandlingService) : BaseController(sender, errorHandlingService)
{

    [HttpGet]
    // [HasPermission(Permissions.AuditLogsRead)]
    public async Task<IActionResult> GetAllAuditLogs(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        GetAllAuditLogsQuery query = new(pageIndex, pageSize, cancellationToken);
        Result<IPaginatedList<GetAllAuditLogsQueryResponse>> result = await _sender.Send(query, cancellationToken);

        return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok(result.Value);
    }

    [HttpPost("dynamic")]
    // [HasPermission(Permissions.AuditLogsRead)]
    public async Task<IActionResult> GetAllAuditLogsDynamic(
        [FromBody] DynamicQuery dynamicQuery,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        GetAllAuditLogsDynamicQuery query = new(pageIndex, pageSize, dynamicQuery);
        Result<IPaginatedList<GetAllAuditLogsDynamicQueryResponse>> result = await _sender.Send(query, cancellationToken);

        return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok(result.Value);
    }
}
