using System.Text.Json;
using Microsoft.Extensions.Options;
using Serilog;
using Confluent.Kafka;
using Bamboo.Notifications.Template.Domain.Entities;
using Bamboo.Notifications.Template.Application.Contracts;
using Bamboo.Notifications.Template.Application.Configurations;

namespace Bamboo.Notifications.Template.Infrastructure.Services;

public class BrokerConsumer : IBrokerConsumer
{
    private static BrokerConfig _brokerConfig;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly ILogger _logger;

    public BrokerConsumer(IOptions<BrokerConfig> brokerConfigOptions, ILogger logger)
    {
        _brokerConfig = brokerConfigOptions.Value;
        _logger = logger;
        _consumer = BuildConsumer(_logger);
        SubscribeConsumer();
    }

    public void Consume(Func<Payload, Task<bool>> process, CancellationToken cancellationToken = default)
    {
        try
        {
            var consumeResult = _consumer.Consume(cancellationToken);
            _logger.Information($"New kafka message: {JsonSerializer.Serialize(consumeResult)}");
            _ = process(new Payload(consumeResult.Topic, consumeResult.Message.Value)).Result;
            _consumer.Commit(consumeResult);
            _logger.Information($"Kafka message commited, Topic:{consumeResult.Topic} - Offset:{consumeResult.Offset.Value}");
        }
        catch (Exception e)
        {
            _logger.Fatal(e, "Error in BrokerConsumer");
            throw;
        }
    }

    public void Close() => _consumer.Close();

    private void SubscribeConsumer()
    {
        _consumer.Subscribe(_brokerConfig.ConsumerConfig.Topics);

        foreach (var topic in _brokerConfig.ConsumerConfig.Topics)
        {
            _logger.Information($"Subscribed to {topic}.");
        }
    }

    private static IConsumer<Ignore, string> BuildConsumer(ILogger logger)
    {
        return new ConsumerBuilder<Ignore, string>(GetConsumerConfig())
            .SetValueDeserializer(Deserializers.Utf8)
            .SetErrorHandler((_, e) => logger.Error($"Consumer Error: {e.Reason}"))
            .Build();
    }

    private static ConsumerConfig GetConsumerConfig() =>
           _brokerConfig.UserName switch
           {
               null or "" => new ConsumerConfig
               {
                   BootstrapServers = _brokerConfig.BootstrapServers,
                   GroupId = _brokerConfig.ConsumerConfig.GroupId,
                   EnableAutoOffsetStore = _brokerConfig.ConsumerConfig.EnableAutoOffsetStore,
                   EnableAutoCommit = _brokerConfig.ConsumerConfig.EnableAutoCommit,
                   MaxPollIntervalMs = _brokerConfig.ConsumerConfig.MaxPollIntervalMs
               },
               _ => new ConsumerConfig
               {
                   BootstrapServers = _brokerConfig.BootstrapServers,
                   GroupId = _brokerConfig.ConsumerConfig.GroupId,
                   EnableAutoOffsetStore = _brokerConfig.ConsumerConfig.EnableAutoOffsetStore,
                   EnableAutoCommit = _brokerConfig.ConsumerConfig.EnableAutoCommit,
                   MaxPollIntervalMs = _brokerConfig.ConsumerConfig.MaxPollIntervalMs,
                   SecurityProtocol = SecurityProtocol.SaslSsl,
                   SaslMechanism = SaslMechanism.ScramSha512,
                   SaslUsername = _brokerConfig.UserName,
                   SaslPassword = _brokerConfig.Password,
               },
           };
}