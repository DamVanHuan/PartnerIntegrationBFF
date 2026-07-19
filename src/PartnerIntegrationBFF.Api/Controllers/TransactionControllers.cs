using Microsoft.AspNetCore.Mvc;
using PartnerIntegrationBFF.Api.Securities;
using PartnerIntegrationBFF.Application.DTOs.Transactions;
using PartnerIntegrationBFF.Application.Services.Transactions;

namespace PartnerIntegrationBFF.Api.Controllers;

[ApiController]
[Route("api/v1/partner/transactions")]
[ApiKeyAuthorize]
public class TransactionControllers : ControllerBase
{
  private readonly ITransactionService _transactionService;

  public TransactionControllers(ITransactionService transactionService)
  {
    _transactionService = transactionService;
  }

  [HttpPost]
  public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
  {
    var result = await _transactionService.CreateTransactionAsync(request, cancellationToken);
    return Ok(result);
  }
}