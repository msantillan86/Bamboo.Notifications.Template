using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;
using Bamboo.Serilog.Context;
using Bamboo.Notifications.Template.Application.Contracts;
using Bamboo.Notifications.Template.Domain.Entities.Crosscutting;
using Bamboo.Notifications.Template.Application.Handlers;

namespace Bamboo.Notifications.Template.Application.Services;

public class ListenerService : IListenerService
{
    private readonly ILogger _logger;
    private readonly IBrokerConsumer _brokerConsumer;
    private readonly IServiceProvider _serviceProvider;

    public ListenerService(
        IBrokerConsumer brokerConsumer,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        _brokerConsumer = brokerConsumer;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task DoWork(CancellationToken stoppingToken)
    {
        _logger.Information("Consumer ready to receive messages");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scopedService = _serviceProvider.CreateScope();
                var logContext = GetBambooSerilogContext(scopedService);
                using (LogContext.PushProperty(logContext.LogHashKeyName, logContext.LogHashKey, true))
                {
                    _brokerConsumer.Consume(async message =>
                    {
                        var command = GetEventCommand(message.Topic, scopedService);
                        return await command.Handle(message.Content);
                    }, stoppingToken);
                    _logger.Information(logContext.AsJsonText());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error handling incoming message: {ex.Message}");
            }
        }
        return Task.CompletedTask;
    }

    private ICommand GetEventCommand(string topic, IServiceScope scopedService)
    {
        return topic switch
        {
            "Gateway-Event-TransactionCompleted" => scopedService.ServiceProvider.GetService<TransactionCompletedHandler>(),
            _ => throw new NotImplementedException($"Topic {topic} not implemented."),
        };
    }

    private IBambooSerilogContext GetBambooSerilogContext(IServiceScope scopedService)
    {
        var bambooSerilogContext = scopedService.ServiceProvider.GetService<IBambooSerilogContext>();
        bambooSerilogContext!.LogHashKey = Guid.NewGuid().ToString();
        bambooSerilogContext.LogHashKeyName = "LogHashKey";
        return bambooSerilogContext;
    }

    public void Dispose() => _brokerConsumer.Close();
}