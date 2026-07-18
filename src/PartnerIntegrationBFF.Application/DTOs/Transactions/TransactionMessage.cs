namespace PartnerIntegrationBFF.Application.DTOs.Transactions;

public record TransactionMessage(string PartnerId, string TransactionReference, decimal Amount, string Currency, DateTimeOffset Timestamp);

