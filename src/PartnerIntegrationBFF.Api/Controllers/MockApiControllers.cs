using Microsoft.AspNetCore.Mvc;

namespace PartnerIntegrationBFF.Api.Controllers;

[ApiController]
[Route("api/mock")]
public class MockApiController : ControllerBase
{
  private static readonly Random Random = new();

  [HttpGet("partners/{partnerId}/verify")]
  public IActionResult Verify(string partnerId)
  {
    if (Random.Next(100) < 30)
    {
      throw new TimeoutException($"Partner timeout {partnerId}");
    }

    return Ok(true);
  }
}