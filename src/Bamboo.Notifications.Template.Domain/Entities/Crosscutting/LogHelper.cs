using System.Text.Json;
using Serilog;
using ProtocolManager.Contracts;
using Bamboo.Serilog.Context;
using Bamboo.Serilog.Context.Entities;

namespace Bamboo.Notifications.Template.Domain.Entities.Crosscutting;

public static class LogHelper
{
    public static async Task<TResult> LogRequestResponse<TResult>(this ILogger log, ProtocolRequest request, Func<Task<TResult>> fnc)
    {
        log.Information($"Request: {request.AsJsonText()}");
        var response = await fnc();
        log.Information($"Response: {response?.AsJsonText()}");
        return response;
    }

    public static string AsJsonText(this object obj)
    {
        try
        {
            var response = JsonSerializer.Serialize(obj);
            return response;
        }
        catch { };
        return string.Empty;
    }

    public static DataProviderConnectionLog GetDataProviderConnectionLog(string providerName, string urlApi, string urlEndpoint) =>
        new()
        {
            ProviderName = providerName,
            ProviderType = ProviderTypes.MessageQueue,
            Comments = $"URL={urlApi}",
            Action = $"GET {urlApi}{urlEndpoint}"
        };

    public static ProtocolRequest SetProtocolRequest(IBambooSerilogContext bambooSerilogContext, string url, RequestFormat requestFormat, RequestMethod requestMethod)
    {
        var protocolRequest = new ProtocolRequest()
        {
            Headers = new Dictionary<string, string>(),
            RequestFormat = requestFormat,
            RequestMethod = requestMethod,
            RequestUrl = url,
        };
        protocolRequest.Headers.Add(bambooSerilogContext.LogHashKeyName ?? "LogHashKey", bambooSerilogContext.LogHashKey ?? Guid.NewGuid().ToString());
        return protocolRequest;
    }
}