using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Web.Controllers.Api;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace AppTemplate.Web.Controllers;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SecurityController : BaseController
{
    private readonly IAntiforgery _antiforgery;
    public SecurityController(
        ISender sender,
        IErrorHandlingService errorHandlingService,
        IAntiforgery antiforgery)
        : base(sender, errorHandlingService)
    {
        _antiforgery = antiforgery;
    }

    [HttpGet("antiforgery/token")]
  public IActionResult GetToken()
  {
    var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
    return new JsonResult(new
    {
      token = tokens.RequestToken
    });
  }
}

