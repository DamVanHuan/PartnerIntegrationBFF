using PartnerIntegrationBFF.Application.DTOs.Transactions;
using PartnerIntegrationBFF.Application.Validators.Transactions;

namespace PartnerIntegrationBFF.Application.Unitests.Validators;

public class CreateTransactionValidatorTests
{
  private readonly CreateTransactionValidator _validator = new();

  public static IEnumerable<object[]> InvalidTimestamps =>
    new List<object[]>
    {
        new object[] { default(DateTimeOffset) },
        new object[] { DateTimeOffset.UtcNow.AddSeconds(1) },
    };

  private static TransactionRequest ValidRequest() => new(
      PartnerId: "P-1001",
      TransactionReference: "TXN-99823",
      Amount: 250.00m,
      Currency: "USD",
      Timestamp: DateTimeOffset.UtcNow.AddSeconds(-1));

  [Fact]
  public async Task Should_Pass_For_A_Fully_Valid_Request()
  {
    var result = await _validator.ValidateAsync(ValidRequest());

    Assert.True(result.IsValid);
  }

  [Theory]
  [InlineData("")]
  [InlineData(null)]
  public async Task Should_Fail_When_PartnerId_Is_Missing(string? partnerId)
  {
    var request = ValidRequest() with { PartnerId = partnerId! };

    var result = await _validator.ValidateAsync(request);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(TransactionRequest.PartnerId));
  }

  [Theory]
  [InlineData("")]
  [InlineData(null)]
  public async Task Should_Fail_When_TransactionReference_Is_Missing(string? transactionReference)
  {
    var request = ValidRequest() with { TransactionReference = transactionReference! };

    var result = await _validator.ValidateAsync(request);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(TransactionRequest.TransactionReference));
  }


  [Theory]
  [InlineData(0)]
  [InlineData(-50)]
  public async Task Should_Fail_When_Amount_Is_Not_Greater_Than_Zero(decimal amount)
  {
    var request = ValidRequest() with { Amount = amount };

    var result = await _validator.ValidateAsync(request);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(TransactionRequest.Amount));
  }

  [Theory]
  [InlineData("")]
  [InlineData(null)]
  [InlineData("XXX")]
  public async Task Should_Fail_When_Currency_Is_Missing_Or_Unsupported(string? currency)
  {
    var request = ValidRequest() with { Currency = currency! };

    var result = await _validator.ValidateAsync(request);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(TransactionRequest.Currency));
  }

  [Theory]
  [MemberData(nameof(InvalidTimestamps))]
  public async Task Should_Fail_When_Timestamp_Is_Invalid(DateTimeOffset timestamp)
  {
    var request = ValidRequest() with { Timestamp = timestamp };

    var result = await _validator.ValidateAsync(request);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(TransactionRequest.Timestamp));
  }
}