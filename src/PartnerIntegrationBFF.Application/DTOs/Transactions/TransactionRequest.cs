namespace PartnerIntegrationBFF.Application.DTOs.Transactions;

public record TransactionRequest(string PartnerId, string TransactionReference, decimal Amount, string Currency, DateTimeOffset Timestamp);

