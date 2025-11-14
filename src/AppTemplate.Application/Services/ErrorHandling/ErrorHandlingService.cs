using System.Globalization;
using System.Net;
using AppTemplate.Application.Services.Localization;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppTemplate.Application.Services.ErrorHandling;

public class ErrorHandlingService : IErrorHandlingService
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ILocalizationService _localizationService;

  public ErrorHandlingService(IHttpContextAccessor httpContextAccessor, ILocalizationService localizationService)
  {
    _httpContextAccessor = httpContextAccessor;
    _localizationService = localizationService;
  }

  public IActionResult HandleErrorResponse<T>(Result<T> result)
  {
    var errorList = result.Errors.ToList();
    var validationErrors = result.ValidationErrors.Select(ve => ve.ErrorMessage).ToList();
    var combinedErrors = errorList.Concat(validationErrors).ToList();

    var language = GetLanguageFromHeader();
    var summaryMessage = GetSummaryMessage(result.Status, language);

    var problemDetails = new ProblemDetails
    {
      Title = GetTitle(result.Status),
      Status = GetHttpStatusCode(result.Status),
      Detail = summaryMessage,
      Instance = _httpContextAccessor.HttpContext?.Request.Path,
      Type = GetTypeUri(result.Status)
    };

    problemDetails.Extensions.Add("errors", combinedErrors);
    problemDetails.Extensions.Add("traceId", _httpContextAccessor.HttpContext?.TraceIdentifier);

    return new ObjectResult(problemDetails)
    {
      StatusCode = problemDetails.Status
    };
  }

  private string GetLanguageFromHeader()
  {
    var acceptLanguageHeader = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString();
    if (!string.IsNullOrEmpty(acceptLanguageHeader))
    {
      var languages = acceptLanguageHeader.Split(',');
      if (languages.Length > 0)
      {
        return languages[0];
      }
    }
    return CultureInfo.CurrentCulture.Name; // Fallback to current culture
  }

  private string GetSummaryMessage(ResultStatus status, string language)
  {
    var key = status switch
    {
      ResultStatus.NotFound => "Errors.NotFound",
      ResultStatus.Invalid => "Errors.Invalid",
      ResultStatus.Error => "Errors.Error",
      ResultStatus.Forbidden => "Errors.Forbidden",
      ResultStatus.Unauthorized => "Errors.Unauthorized",
      ResultStatus.Conflict => "Errors.Conflict",
      _ => "Errors.Error"
    };

    return _localizationService.GetLocalizedString(key, language);
  }

  private string GetTitle(ResultStatus status) =>
      status switch
      {
        ResultStatus.NotFound => "Resource Not Found",
        ResultStatus.Invalid => "Validation Error",
        ResultStatus.Error => "An Unexpected Error Occurred",
        ResultStatus.Forbidden => "Forbidden",
        ResultStatus.Unauthorized => "Unauthorized Access",
        ResultStatus.Conflict => "Conflict",
        _ => "An Unexpected Error Occurred"
      };

  private int GetHttpStatusCode(ResultStatus status) =>
      status switch
      {
        ResultStatus.NotFound => (int)HttpStatusCode.NotFound,
        ResultStatus.Invalid => (int)HttpStatusCode.BadRequest,
        ResultStatus.Error => (int)HttpStatusCode.InternalServerError,
        ResultStatus.Forbidden => (int)HttpStatusCode.Forbidden,
        ResultStatus.Unauthorized => (int)HttpStatusCode.Unauthorized,
        ResultStatus.Conflict => (int)HttpStatusCode.Conflict,
        _ => (int)HttpStatusCode.InternalServerError
      };

  private string GetTypeUri(ResultStatus status) =>
      status switch
      {
        ResultStatus.NotFound => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
        ResultStatus.Invalid => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
        ResultStatus.Error => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
        ResultStatus.Forbidden => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
        ResultStatus.Unauthorized => "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
        ResultStatus.Conflict => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
        _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
      };
}
