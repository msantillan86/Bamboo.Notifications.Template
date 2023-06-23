using System.Text.Json;
using Microsoft.Extensions.Options;
using Serilog;
using Bamboo.SQSForRetries.Contracts;
using Bamboo.SQSForRetries.Models;
using Bamboo.Notifications.Template.Application.Contracts;
using Bamboo.Notifications.Template.Domain.Entities.Events;
using Bamboo.Notifications.Template.Application.Configurations;
using ProtocolManager.Contracts;

namespace Bamboo.Notifications.Template.Application.Handlers;

public class TransactionCompletedRetryHandler : IRetryHandler<RetryEvent>
{
    private readonly short[] _retries;
    private readonly ITemplateService _templateService;
    private readonly ILogger _logger;

    public TransactionCompletedRetryHandler(
        IOptions<BrokerConfig> brokerConfigOptions,
        ITemplateService templateService,
        ILogger logger)
    { 
        _retries = brokerConfigOptions.Value.ConsumerConfig.Retries;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<HandleResult> Handle(RetryEvent message, CancellationToken cancellationToken)
    {
        var response = new HandleResult();
        response.ResultAction = ResultAction.Success;
        var maxRetries = _retries.Length + 1;
        if (message.ReceiveCount >= maxRetries)
        {
            _logger.Error($"The message has reached the max allowed retries. Message: {JsonSerializer.Serialize(message)}");
            response.ResultAction = ResultAction.Dismiss;
            return await Task.FromResult(response);
        }
        var request = new CompleteEventWebModelRequest(message.PurchaseId);
        var success = await _templateService.SendPurchaseAsync(request);
        if (success is false)
        {
            _logger.Warning($"Message Failed, retrying.");
            response.ResultAction = ResultAction.Reprocess;
            response.NewVisibilityTimeout = _retries[message.ReceiveCount - 1];
        }

        return await Task.FromResult(response);
    }
}