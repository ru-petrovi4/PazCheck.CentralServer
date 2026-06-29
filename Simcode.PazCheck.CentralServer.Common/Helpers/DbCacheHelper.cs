using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public static class DbCacheHelper
    {
        public static bool CheckAccess(string[]? apiFunctionRoles, string[] roles)
        {
            if (apiFunctionRoles is not null)
            {
                if (apiFunctionRoles.Contains(DefaultRoleBusinessFunctions.Public_RoleApiFunction_Modifier, StringComparer.InvariantCultureIgnoreCase))
                {                    
                    return true;
                }
                if (apiFunctionRoles.Contains(DefaultRoleBusinessFunctions.AllRoles_RoleApiFunction_Modifier, StringComparer.InvariantCultureIgnoreCase) &&
                    roles.Length > 0)
                {                    
                    return true;
                }
                foreach (var role in roles)
                {
                    if (apiFunctionRoles.Contains(role, StringComparer.InvariantCultureIgnoreCase))
                        return true;
                }
            }

            return false;
        }
    }
}
