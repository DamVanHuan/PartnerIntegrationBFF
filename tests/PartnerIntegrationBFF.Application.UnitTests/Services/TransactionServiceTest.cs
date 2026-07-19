using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PartnerIntegrationBFF.Application.DTOs.Transactions;
using PartnerIntegrationBFF.Application.Interfaces;
using PartnerIntegrationBFF.Application.Services.Transactions;

namespace PartnerIntegrationBFF.Application.Unitests.Services;

public class TransactionServiceTest
{
    private readonly IValidator<CreateTransactionRequest> _validator = Substitute.For<IValidator<CreateTransactionRequest>>();
    private readonly IPartnerVerificationClient _verificationService = Substitute.For<IPartnerVerificationClient>();
    private readonly ITransactionQueuePublisher _queuePublisher = Substitute.For<ITransactionQueuePublisher>();

    private TransactionServices CreateService() =>
        new(_validator, _verificationService, _queuePublisher);

    private static CreateTransactionRequest ValidRequest() => new(
        PartnerId: "P-1001",
        TransactionReference: "TXN-99823",
        Amount: 250.00m,
        Currency: "USD",
        Timestamp: DateTimeOffset.UtcNow.AddSeconds(-1));

    private void SetupValidationResult(bool isValid, params ValidationFailure[] failures)
    {
        _validator.ValidateAsync(Arg.Any<CreateTransactionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(isValid ? Array.Empty<ValidationFailure>() : failures));
    }

    [Fact]
    public async Task Should_Publish_And_Return_Success_When_Partner_Is_Verified()
    {
        SetupValidationResult(isValid: true);
        _verificationService.VerifyPartnerAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var service = CreateService();
        var validRequest = ValidRequest();
        var result = await service.CreateTransactionAsync(validRequest, CancellationToken.None);

        Assert.True(result.Success);
        await _queuePublisher.Received(1).PublishTransactionAsync(
            Arg.Is<CreateTransactionMessage>(m => m.PartnerId == validRequest.PartnerId
            && m.TransactionReference == validRequest.TransactionReference
            && m.Amount == validRequest.Amount
            && m.Currency == validRequest.Currency
            && m.Timestamp == validRequest.Timestamp),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Throw_ValidationException_When_Request_Is_Invalid()
    {
        SetupValidationResult(isValid: false, new ValidationFailure("PartnerId", "partnerId is required"));

        var service = CreateService();

        await Assert.ThrowsAsync<ValidationException>(
            () => service.CreateTransactionAsync(ValidRequest(), CancellationToken.None));

        // Không được tiếp tục gọi verification hay publish nếu request không hợp lệ
        await _verificationService.DidNotReceive().VerifyPartnerAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _queuePublisher.DidNotReceive().PublishTransactionAsync(Arg.Any<CreateTransactionMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Reject_Without_Publishing_When_Partner_Is_Not_Verified()
    {
        SetupValidationResult(isValid: true);
        _verificationService.VerifyPartnerAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var service = CreateService();
        var result = await service.CreateTransactionAsync(ValidRequest(), CancellationToken.None);

        Assert.False(result.Success);
        await _queuePublisher.DidNotReceive().PublishTransactionAsync(Arg.Any<CreateTransactionMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Verification_Fails_After_Resilience()
    {
        SetupValidationResult(isValid: true);

        _verificationService.VerifyPartnerAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException());

        var service = CreateService();

        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.CreateTransactionAsync(ValidRequest(), CancellationToken.None));

        await _queuePublisher.DidNotReceive().PublishTransactionAsync(Arg.Any<CreateTransactionMessage>(), Arg.Any<CancellationToken>());
    }
}