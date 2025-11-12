using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AppTemplate.Presentation.Middlewares;

public sealed class ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
{
  private readonly RequestDelegate _next = next;
  private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

  private static readonly Action<ILogger, string, Exception> _logError =
      LoggerMessage.Define<string>(
          LogLevel.Error,
          new EventId(1, nameof(ExceptionHandlingMiddleware)),
          "Exception occurred: {Message}");

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception exception)
    {
      _logError(_logger, exception.Message, exception);

      if (!context.Response.HasStarted)
      {
        ExceptionDetails exceptionDetails = GetExceptionDetails(exception);

        ProblemDetails problemDetails = new()
        {
          Status = exceptionDetails.Status,
          Type = exceptionDetails.Type,
          Title = exceptionDetails.Title,
          Detail = exceptionDetails.Detail,
        };

        if (exceptionDetails.Errors is not null)
        {
          problemDetails.Extensions["errors"] = exceptionDetails.Errors;
        }

        context.Response.StatusCode = exceptionDetails.Status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(problemDetails);
      }
      else
      {
        _logger.LogWarning("The response has already started, the exception handling middleware will not modify the response.");
      }
    }
  }

  private static ExceptionDetails GetExceptionDetails(Exception exception)
  {
    return exception switch
    {
      ValidationException validationException => new ExceptionDetails(
          StatusCodes.Status400BadRequest,
          "ValidationFailure",
          "Validation error",
          "One or more validation errors has occurred",
          validationException.Errors),
      _ => new ExceptionDetails(
          StatusCodes.Status500InternalServerError,
          "ServerError",
          "Server error",
          "An unexpected error has occurred",
          null)
    };
  }

  internal sealed record ExceptionDetails(
      int Status,
      string Type,
      string Title,
      string Detail,
      IEnumerable<object>? Errors);
}
