using Bamboo.Notifications.Template.Domain.Entities;

namespace Bamboo.Notifications.Template.Application.Contracts;

public interface IBrokerConsumer
{
    void Consume(Func<Payload, Task<bool>> process, CancellationToken cancellationToken = default);
    void Close();
}