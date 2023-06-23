using Bamboo.Notifications.Template.Domain.Entities.Events;
using ProtocolManager.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bamboo.Notifications.Template.Application.Contracts
{
    public interface ITemplateService
    {
        Task<bool> SendPurchaseAsync(CompleteEventWebModelRequest request);
    }
}
