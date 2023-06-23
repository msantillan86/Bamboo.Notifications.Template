namespace Bamboo.Notifications.Template.Domain.Entities.Events;

public record TransactionCompletedEvent
{
    public long PurchaseId { get; init; }
    public int CommerceAccountId { get; init; }
}
