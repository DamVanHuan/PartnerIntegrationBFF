using PartnerIntegrationBFF.Application.DTOs.Transactions;

namespace PartnerIntegrationBFF.Application.Services.Transactions;

public interface ITransactionService
{
  Task<CreateTransactionResponse> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken);
}