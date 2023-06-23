namespace Bamboo.Notifications.Template.Domain.Entities;

public record Payload
{
    public Payload(string topic, string content)
    {
        Topic = topic;
        Content = content;
    }

    public string Topic { get; init; }
    public string Content { get; init; }
}