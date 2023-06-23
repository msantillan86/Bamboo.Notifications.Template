using Bamboo.SQSForRetries.Contracts;

namespace Bamboo.Notifications.Template.Domain.Entities.Events;

public record RetryEvent : ISqsMessage
{
    public long PurchaseId { get; set; }
    public int ReceiveCount { get; set; }
}