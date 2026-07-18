using PartnerIntegrationBFF.Application.DTOs.Transactions;

namespace PartnerIntegrationBFF.Application.Services.Transactions;

public interface ITransactionService
{
  Task<TransactionResponse> CreateTransaction(TransactionRequest request, CancellationToken cancellationToken);
}