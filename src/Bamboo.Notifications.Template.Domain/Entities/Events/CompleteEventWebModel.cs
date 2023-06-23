namespace Bamboo.Notifications.Template.Domain.Entities.Events;

public class CompleteEventWebModelRequest
{
    public CompleteEventWebModelRequest(long purchaseId)
    {
        PurchaseId = purchaseId;
    }
    public long PurchaseId { get; set; }
}
