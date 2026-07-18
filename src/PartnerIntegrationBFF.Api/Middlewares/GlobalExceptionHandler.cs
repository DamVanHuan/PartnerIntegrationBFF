using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PartnerIntegrationBFF.Api.Middlewares;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
  public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
  {
    switch (exception)
    {
      case ValidationException validationException:
        await HandleValidationException(httpContext, validationException, cancellationToken);
        break;

      case TimeoutException:
        await WriteProblem(
            httpContext,
            StatusCodes.Status503ServiceUnavailable,
            "Partner service timed out",
            cancellationToken);
        break;

      case HttpRequestException:
        await WriteProblem(
            httpContext,
            StatusCodes.Status503ServiceUnavailable,
            "Partner service is unavailable",
            cancellationToken);
        break;

      default:
        await WriteProblem(
               httpContext,
               StatusCodes.Status500InternalServerError,
               "An unexpected error occurred, please try again later",
               cancellationToken);

        break;
    }

    return true;
  }

  private async Task WriteProblem(HttpContext context, int statusCode, string title, CancellationToken cancellationToken)
  {
    context.Response.StatusCode = statusCode;
    var problem = new ProblemDetails
    {
      Title = title,
      Status = statusCode
    };
    await context.Response.WriteAsJsonAsync(problem, cancellationToken);
  }

  private static async Task HandleValidationException(
    HttpContext context,
    ValidationException exception,
    CancellationToken cancellationToken)
  {
    var errors = exception.Errors
        .GroupBy(x => JsonNamingPolicy.CamelCase.ConvertName(x.PropertyName))
        .ToDictionary(
            g => g.Key,
            g => g.Select(x => x.ErrorMessage).ToArray());

    var problem = new ValidationProblemDetails(errors)
    {
      Title = "Validation failed",
      Status = StatusCodes.Status400BadRequest
    };

    context.Response.StatusCode = StatusCodes.Status400BadRequest;

    await context.Response.WriteAsJsonAsync(problem, cancellationToken);
  }
}