using Microsoft.Extensions.Hosting;
using Bamboo.Notifications.Template.Application.Contracts;

namespace Bamboo.Notifications.Template.HostedServices;

public class HostedService : BackgroundService
{
    private readonly IListenerService _listenerService;

    public HostedService(IListenerService listenerService)
        => _listenerService = listenerService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        => await _listenerService.DoWork(stoppingToken);

    public override void Dispose()
    {
        _listenerService.Dispose();
        base.Dispose();
    }
}