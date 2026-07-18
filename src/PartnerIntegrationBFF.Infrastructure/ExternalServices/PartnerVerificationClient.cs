using PartnerIntegrationBFF.Application.Interfaces;

namespace PartnerIntegrationBFF.Infrastructure.ExternalServices;

public class PartnerVerificationClient : IPartnerVerificationClient
{
  private readonly HttpClient _httpClient;

  public PartnerVerificationClient(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<bool> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken)
  {
    var response = await _httpClient.GetAsync($"/api/mock/partners/{partnerId}/verify", cancellationToken);
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadAsStringAsync(cancellationToken);
    return bool.Parse(result);
  }
}