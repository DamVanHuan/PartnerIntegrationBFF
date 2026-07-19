using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using PartnerIntegrationBFF.Application.Interfaces;
using PartnerIntegrationBFF.Infrastructure.ExternalServices;
using PartnerIntegrationBFF.Infrastructure.Messagings;
using Polly;

namespace PartnerIntegrationBFF.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {

    services.AddHttpClient<IPartnerVerificationClient, PartnerVerificationClient>(client =>
    {
      client.BaseAddress = new Uri(configuration["PartnerApi:BaseUrl"]);
    })
    .AddResilienceHandler("partner-verification", builder =>
    {
      builder.AddRetry(new HttpRetryStrategyOptions
      {
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential,
        Delay = TimeSpan.FromMilliseconds(200)
      });

      builder.AddTimeout(TimeSpan.FromSeconds(5));
    });

    services.Configure<RabbitMqOption>(
    configuration.GetSection(RabbitMqOption.SectionName));

    services.AddScoped<ITransactionQueuePublisher, RabbitMqPublisher>();

    return services;
  }
}