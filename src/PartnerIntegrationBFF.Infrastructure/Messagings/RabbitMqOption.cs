namespace PartnerIntegrationBFF.Infrastructure.Messagings;

public class RabbitMqOption
{
  public const string SectionName = "RabbitMq";

  public string Host { get; init; } = string.Empty;

  public int Port { get; init; }

  public string Username { get; init; } = string.Empty;

  public string Password { get; init; } = string.Empty;

  public string QueueName { get; init; } = string.Empty;
}