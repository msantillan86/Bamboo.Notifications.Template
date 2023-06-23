using System.Text.Json;
using Serilog;
using Bamboo.SQSForRetries.Contracts;
using Bamboo.Notifications.Template.Application.Contracts;
using Bamboo.Notifications.Template.Domain.Entities.Events;
using Bamboo.Notifications.Template.Domain.Entities.Crosscutting;
using ProtocolManager.Contracts;

namespace Bamboo.Notifications.Template.Application.Handlers;

public class TransactionCompletedHandler : ICommand
{
    private readonly ILogger _logger;
    private readonly ISqsPublisherService _publisherService;
    private readonly IAntifraudService _AntifraudService;

    public TransactionCompletedHandler(
        IAntifraudService AntifraudService,
        ISqsPublisherService publisherService,
        ILogger logger)
    {
        _AntifraudService = AntifraudService;
        _publisherService = publisherService;
        _logger = logger;
    }

    public async Task<bool> Handle(string rawMessage)
    {
        var validatedResult = DeserializeMessage(rawMessage);
        if (validatedResult.IsFailure)
        {
            _logger.Error(validatedResult.Error);
            return false;
        }
        var request = new CompleteEventWebModelRequest(validatedResult.Value.PurchaseId);
        var success = await _AntifraudService.SendPurchaseAsync(request);
        if (success is false)
        {
            _logger.Warning($"Antifraud response failed, ready for retry.");
            await _publisherService.PublishAsync(new RetryEvent() { PurchaseId = validatedResult.Value.PurchaseId });
        }
        return success;
    }

    private static Result<TransactionCompletedEvent> DeserializeMessage(string rawMessage)
    {
        try
        {
            var message = JsonSerializer.Deserialize<TransactionCompletedEvent>(rawMessage);
            if (message is not { CommerceAccountId: > 0, PurchaseId: > 0 })
            {
                return Result.Fail<TransactionCompletedEvent>($"Invalid fields in rawMessage: {rawMessage}");
            }
            return Result.Ok(message);
        }
        catch (Exception ex)
        {
            return Result.Fail<TransactionCompletedEvent>($"InvalidFormat more info: {ex.Message}");
        }

    }
}