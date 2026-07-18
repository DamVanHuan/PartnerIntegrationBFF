using FluentValidation;
using PartnerIntegrationBFF.Application.DTOs.Transactions;

namespace PartnerIntegrationBFF.Application.Validators.Transactions;

public class CreateTransactionValidator : AbstractValidator<TransactionRequest>
{
  private static readonly HashSet<string> ValidCurrencies = new(StringComparer.OrdinalIgnoreCase)
  {
      "VND", "USD", "EUR", "JPY", "AUD", "CAD"
  };

  public CreateTransactionValidator()
  {
    RuleFor(x => x.PartnerId)
      .NotEmpty()
      .WithMessage("partnerId is required");

    RuleFor(x => x.TransactionReference)
      .NotEmpty()
      .WithMessage("transactionReference is required");

    RuleFor(x => x.Amount)
      .GreaterThan(0)
      .WithMessage("amount must be greater than 0");

    RuleFor(x => x.Currency)
      .NotEmpty()
      .WithMessage("currency is required")
      .Must(c => ValidCurrencies.Contains(c))
      .WithMessage("currency is not valid");

    RuleFor(x => x.Timestamp)
      .NotEqual(default(DateTimeOffset))
      .WithMessage("timestamp is required")
      .LessThanOrEqualTo(DateTime.UtcNow)
      .WithMessage("timestamp cannot be in the future");
  }
}

