using Microsoft.Extensions.Options;
using ProtocolManager.Contracts;
using Serilog;
using Bamboo.Serilog.Context;
using Bamboo.Notifications.Template.Application.Contracts;
using Bamboo.Notifications.Template.Domain.Entities.Crosscutting;
using Bamboo.Notifications.Template.Domain.Entities.Events;
using Bamboo.Notifications.Template.Application.Configurations;

namespace Bamboo.Notifications.Template.Infrastructure.Services;

public class TemplateService : ITemplateService
{
    private readonly ServicesConfig _servicesConfig;
    private readonly IBambooSerilogContext _bambooSerilogContext;
    private readonly IProtocolManager _protocolManager;
    private readonly ILogger _logger;

    public TemplateService(
        IOptions<ServicesConfig> servicesConfig,
        IBambooSerilogContext bambooSerilogContext,
        IProtocolManager protocolManager,
        ILogger logger)
    {
        _logger = logger;
        _servicesConfig = servicesConfig.Value;
        _bambooSerilogContext = bambooSerilogContext;
        _protocolManager = protocolManager;
    }

    public async Task<bool> SendPurchaseAsync(CompleteEventWebModelRequest request)
    {

        var notificationResponse = await SendNotificationToTemplate(new ProtocolRequest()
        {
            RequestUrl = _servicesConfig.TemplateUrl,
            RequestMethod = RequestMethod.POST,
            RequestFormat = RequestFormat.JSON,
            RequestMessage = request,
            Resource = "notifications"
        });

        return notificationResponse.ErrorList.Any() || notificationResponse.ResponseCode != ResultCodes.Ok;
    }

    private async Task<ProtocolResponse<string>> SendNotificationToTemplate(ProtocolRequest request)
    {
        return await _bambooSerilogContext.InvokeAndLogDataProvider(
            LogHelper.GetDataProviderConnectionLog(nameof(SendNotificationToTemplate), request.RequestUrl, string.Empty), async () =>
            {
                var protocolRequest = LogHelper.SetProtocolRequest(_bambooSerilogContext, request.RequestUrl, request.RequestFormat, request.RequestMethod);
                protocolRequest.RequestMessage = request.RequestMessage;
                protocolRequest.Resource = request.Resource;
                return await _logger.LogRequestResponse(protocolRequest, async () => await _protocolManager.SendMessageAsync<string>(protocolRequest));
            });
    }
}