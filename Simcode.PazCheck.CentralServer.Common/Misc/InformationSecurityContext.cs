using IdentityServer4;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public class InformationSecurityContext
    {
        /// <summary>
        ///     Lower Invariant
        /// </summary>
        public string User { get; init; } = @"";

        public string SourceIpAddress { get; init; } = @"";

        public string SourceHost { get; init; } = @"";

        public static InformationSecurityContext CreateFrom(HttpContext? httpContext)
        {
            InformationSecurityContext informationSecurityInfo = new()
            {
                User = HttpContextHelper.GetUserLowerInvariant(httpContext),
                SourceIpAddress = HttpContextHelper.GetSourceIpAddress(httpContext),
                SourceHost = HttpContextHelper.GetSourceHost(httpContext),
            };
            return informationSecurityInfo;
        }
    }
}
