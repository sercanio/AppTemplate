using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Myrtus.Clarity.Core.WebAPI;
using Myrtus.Clarity.Core.WebAPI.Controllers;

namespace EcoFind.Web.Controllers;
[Route("api/v1.0/[controller]")]
[ApiController]
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
        return new JsonResult(new { token = tokens.RequestToken });
    }
}

