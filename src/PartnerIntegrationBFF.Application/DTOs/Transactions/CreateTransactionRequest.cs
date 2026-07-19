namespace PartnerIntegrationBFF.Application.DTOs.Transactions;

public record CreateTransactionRequest(string PartnerId, string TransactionReference, decimal Amount, string Currency, DateTimeOffset Timestamp);

