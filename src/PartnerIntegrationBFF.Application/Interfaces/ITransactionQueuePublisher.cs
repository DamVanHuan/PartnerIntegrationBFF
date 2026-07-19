using PartnerIntegrationBFF.Application.DTOs.Transactions;

namespace PartnerIntegrationBFF.Application.Interfaces;

public interface ITransactionQueuePublisher
{
  Task PublishTransactionAsync(CreateTransactionMessage transactionMessage, CancellationToken cancellationToken);
}