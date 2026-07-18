using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PartnerIntegrationBFF.Application.Services.Transactions;

namespace PartnerIntegrationBFF.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    services.AddScoped<ITransactionService, TransactionServices>();

    return services;
  }
}