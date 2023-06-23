namespace Bamboo.Notifications.Template.Application.Configurations;

public record BrokerConfig
{
    public string BootstrapServers { get; init; }
    public string UserName { get; init; }
    public string Password { get; init; }
    public BrokerConsumerConfig ConsumerConfig { get; init; }
}

public record BrokerConsumerConfig
{
    public string GroupId { get; init; }
    public string[] Topics { get; init; }
    public short[] Retries { get; init; }
    public int MaxPollIntervalMs { get; init; }
    public bool EnableAutoCommit { get; init; }
    public bool EnableAutoOffsetStore { get; init; }
}
