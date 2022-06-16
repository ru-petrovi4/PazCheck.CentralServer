using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer
{
    internal class AuthorizationHandler : AuthorizationHandler<AuthorizationRequirement>
    {
        #region construction and destruction

        public AuthorizationHandler()
        {

        }

        #endregion

        #region protected functions

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationRequirement requirement)
        {
            if (context.User is null)
                return Task.CompletedTask;

            var permissionss = context.User.Claims.Where(x => x.Type == "Permission" &&
                                                            x.Value == requirement.Permission &&
                                                            x.Issuer == "LOCAL AUTHORITY");
            if (permissionss.Any())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        #endregion        
    }
}