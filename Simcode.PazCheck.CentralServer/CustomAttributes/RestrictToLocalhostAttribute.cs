using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Simcode.PazCheck.CentralServer
{
    public class RestrictToLocalhostAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var remoteIpAddress = context.HttpContext.Connection.RemoteIpAddress;
            if (remoteIpAddress is null || !IPAddress.IsLoopback(remoteIpAddress))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            base.OnActionExecuting(context);
        }
    }
}
