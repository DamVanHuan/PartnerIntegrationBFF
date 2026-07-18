using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PartnerIntegrationBFF.Api.Securities;

public class ApiKeyAuthorizeAttribute : Attribute, IAsyncActionFilter
{
  private const string ApiKeyHeaderName = "X-API-KEY";

  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
    var expectedKey = configuration["Security:ApiKey"];

    if (string.IsNullOrEmpty(expectedKey))
    {
      context.Result = new ObjectResult("API key is not configured on the server")
      {
        StatusCode = StatusCodes.Status500InternalServerError
      };
      return;
    }

    if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey) ||
        !string.Equals(providedKey, expectedKey, StringComparison.Ordinal))
    {
      context.Result = new UnauthorizedObjectResult(new { message = $"Missing or invalid {ApiKeyHeaderName} header" });
      return;
    }

    await next();
  }
}