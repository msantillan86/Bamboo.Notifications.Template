using Bamboo.Notifications.Template.Domain.Entities.Events;
using ProtocolManager.Contracts;

namespace Bamboo.Notifications.Template.UnitTests.Application.Handlers.Base;

public class HandlerTestsBase
{
    internal bool GetAntifraudErrorResponse()
    {
        var protocolResponse = new ProtocolResponse<object>()
        {
            ResponseCode = ResultCodes.InternalServerError,
            ExternalResponseCode = "ExternalCode",
            ExternalResponseMessage = "ExternalMessage",
            ErrorList = new List<Error>()
            {
                new Error(ErrorType.InternalServerError) { Code = "999", Description = "Fake Error" }
            }
        };
        var a = !(protocolResponse.ErrorList.Any() || protocolResponse.ResponseCode != ResultCodes.Ok);
        return a;
    }

    internal bool GetAntifraudOkResponse()
    {
        var protocolResponse = new ProtocolResponse<object>()
        {
            ResponseCode = ResultCodes.Ok,
            ExternalResponseCode = "ExternalCode",
            ExternalResponseMessage = "ExternalMessage",
            ErrorList = new List<Error>()
        };
        return !protocolResponse.ErrorList.Any() || protocolResponse.ResponseCode == ResultCodes.Ok;
    }
}