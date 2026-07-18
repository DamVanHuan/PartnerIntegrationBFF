using Microsoft.AspNetCore.Mvc;
using PartnerIntegrationBFF.Application.DTOs.Transactions;
using PartnerIntegrationBFF.Application.Services.Transactions;

namespace PartnerIntegrationBFF.Api.Controllers;

[ApiController]
[Route("api/v1/partner/transactions")]
public class TransactionControllers : ControllerBase
{
  private readonly ITransactionService _transactionService;

  public TransactionControllers(ITransactionService transactionService)
  {
    _transactionService = transactionService;
  }

  [HttpPost]
  public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequest request, CancellationToken cancellationToken)
  {
    var result = await _transactionService.CreateTransaction(request, cancellationToken);
    return Ok(result);
  }
}