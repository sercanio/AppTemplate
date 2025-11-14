using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;

namespace AppTemplate.Application.Services.ErrorHandling;

public interface IErrorHandlingService
{
  IActionResult HandleErrorResponse<T>(Result<T> result);
}
