using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Simcode.PazCheck.CentralServer.MicroServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public class MainHub : Hub
    {
        public async Task MonitoringSubscribe() // [FromServices] Monitoring monitoring
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, nameof(MonitoringSubscribe));
        }
    }
}
