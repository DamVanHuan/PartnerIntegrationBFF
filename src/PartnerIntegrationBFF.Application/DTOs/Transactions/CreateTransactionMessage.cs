namespace PartnerIntegrationBFF.Application.DTOs.Transactions;

public record CreateTransactionMessage(string PartnerId, string TransactionReference, decimal Amount, string Currency, DateTimeOffset Timestamp);

