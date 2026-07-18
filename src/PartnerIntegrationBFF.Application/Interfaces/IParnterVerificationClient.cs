namespace PartnerIntegrationBFF.Application.Interfaces;

public interface IPartnerVerificationClient
{
  Task<bool> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken);
}