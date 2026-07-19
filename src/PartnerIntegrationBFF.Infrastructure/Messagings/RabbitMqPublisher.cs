using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PartnerIntegrationBFF.Application.DTOs.Transactions;
using PartnerIntegrationBFF.Application.Interfaces;
using RabbitMQ.Client;

namespace PartnerIntegrationBFF.Infrastructure.Messagings;

public class RabbitMqPublisher : ITransactionQueuePublisher
{
  private readonly RabbitMqOption _option;

  public RabbitMqPublisher(IOptions<RabbitMqOption> option)
  {
    _option = option.Value;
  }

  public async Task PublishTransactionAsync(CreateTransactionMessage transactionMessage, CancellationToken cancellationToken)
  {
    var factory = new ConnectionFactory
    {
      HostName = _option.Host,
      Port = _option.Port,
      UserName = _option.Username,
      Password = _option.Password
    };

    await using var connection = await factory.CreateConnectionAsync(cancellationToken);

    await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

    await channel.QueueDeclareAsync(
        queue: _option.QueueName,
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: null,
        cancellationToken: cancellationToken);

    var body = Encoding.UTF8.GetBytes(
        JsonSerializer.Serialize(transactionMessage));

    await channel.BasicPublishAsync(
        exchange: string.Empty,
        routingKey: _option.QueueName,
        mandatory: false,
        body: body,
        cancellationToken: cancellationToken);
  }
}