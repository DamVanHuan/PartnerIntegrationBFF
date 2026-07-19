using PartnerIntegrationBFF.Application.DTOs.Transactions;
using FluentValidation;
using PartnerIntegrationBFF.Application.Interfaces;

namespace PartnerIntegrationBFF.Application.Services.Transactions;

public class TransactionServices : ITransactionService
{
  private readonly IValidator<CreateTransactionRequest> _validator;
  private readonly IPartnerVerificationClient _partnerVerificationClient;
  private readonly ITransactionQueuePublisher _transactionQueuePublisher;

  public TransactionServices(IValidator<CreateTransactionRequest> validator, IPartnerVerificationClient partnerVerificationClient, ITransactionQueuePublisher transactionQueuePublisher)
  {
    _validator = validator;
    _partnerVerificationClient = partnerVerificationClient;
    _transactionQueuePublisher = transactionQueuePublisher;
  }

  public async Task<CreateTransactionResponse> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var isPartnerValid = await _partnerVerificationClient.VerifyPartnerAsync(request.PartnerId, cancellationToken);

    if (!isPartnerValid)
    {
      return new CreateTransactionResponse(false, "Invalid partner ID");
    }

    var message = new CreateTransactionMessage(request.PartnerId, request.TransactionReference, request.Amount, request.Currency, request.Timestamp);
    await _transactionQueuePublisher.PublishTransactionAsync(message, cancellationToken);

    return new CreateTransactionResponse(true, "Transaction created successfully");
  }
}
