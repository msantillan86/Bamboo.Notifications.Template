namespace Bamboo.Notifications.Template.Application.Contracts;

public interface IListenerService
{
    Task DoWork(CancellationToken stoppingToken);
    void Dispose();
}