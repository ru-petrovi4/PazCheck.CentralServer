using IdentityServer4;
using IdentityServer4.Services;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.MicroServices
{
    public class UsersAndRolesInfo : IUsersAndRolesInfo
    {
        #region construction and destruction

        public UsersAndRolesInfo(Cache cache)
        {
            _cache = cache;            
        }        

        #endregion

        #region public functions        

        /// <summary>
        ///     SuperUser Is Enabled
        /// </summary>
        public bool SuperUserIsEnabled => Program.SuperUserIsEnabled;

        /// <summary>
        ///     TestUsers Is Enabled
        /// </summary>
        public bool TestUsersIsEnabled => Program.TestUsersIsEnabled;        

        public Task<string[]> GetAllRolesAsync() => _cache.GetAllRolesAsync();        

        #endregion        

        #region private fields
        
        private readonly Cache _cache;        

        #endregion
    }
}


//private Identifiable<int>? GetEntity(PropertyInfo properyInfo, string[] requestStringParts)
//{
//    if (requestStringParts.Length > 1)
//    {
//        var id = new Any(requestStringParts[1]).ValueAsInt32(false);
//        using (var dbContext = _dbContextFactory.CreateDbContext())
//        {
//            var entitiesDbSet = (IQueryable<Identifiable<int>>)properyInfo.GetValue(dbContext)!;
//            return entitiesDbSet.Where(e => e.Id == id).FirstOrDefault();
//        }
//    }
//    return null;
//}


//public Task<string[]> GetStandardRolesAsync(IEnumerable<string> roles)
//{
//    HashSet<string> standardRoles = new();
//    lock (_syncRoot)
//    {
//        string[]? apiFunctionRoles = null;

//        foreach (var role in roles)
//        {
//            _apiFunctionsAndRoles.TryGetValue("Entity.LogEvent.Read", out apiFunctionRoles);
//            if (apiFunctionRoles!.Contains(role, StringComparer.InvariantCultureIgnoreCase))
//                standardRoles.Add(@"PazCheckIsAdmins");

//            _apiFunctionsAndRoles.TryGetValue("Entity.ProjectVersion.Update", out apiFunctionRoles);
//            if (apiFunctionRoles!.Contains(role, StringComparer.InvariantCultureIgnoreCase))
//            {
//                _apiFunctionsAndRoles.TryGetValue("Method.SaveUnversionedChanges", out apiFunctionRoles);
//                if (apiFunctionRoles!.Contains(role, StringComparer.InvariantCultureIgnoreCase))
//                    standardRoles.Add(@"PazCheckEngineers");

//                _apiFunctionsAndRoles.TryGetValue("Entity.Unit.Update", out apiFunctionRoles);
//                if (apiFunctionRoles!.Contains(role, StringComparer.InvariantCultureIgnoreCase))
//                    standardRoles.Add(@"PazCheckSupervisors");
//            }                                              
//        }
//    }
//    return Task.FromResult(standardRoles.ToArray());
//}