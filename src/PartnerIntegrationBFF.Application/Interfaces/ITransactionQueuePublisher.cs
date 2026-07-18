using PartnerIntegrationBFF.Application.DTOs.Transactions;

namespace PartnerIntegrationBFF.Application.Interfaces;

public interface ITransactionQueuePublisher
{
  Task PublishTransactionAsync(TransactionMessage transactionMessage, CancellationToken cancellationToken);
}