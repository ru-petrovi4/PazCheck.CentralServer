using Microsoft.AspNetCore.Authorization;

namespace Simcode.PazCheck.CentralServer
{
    internal class AuthorizationRequirement : IAuthorizationRequirement
    {
        public string Permission { get; private set; }

        public AuthorizationRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
