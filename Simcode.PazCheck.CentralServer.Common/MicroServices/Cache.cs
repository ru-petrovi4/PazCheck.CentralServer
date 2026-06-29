using IdentityServer4;
using IdentityServer4.Services;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.MicroServices;

public class Cache
{
    #region construction and destruction

    public Cache(
        IConfiguration configuration,
        IDbContextFactory<PazCheckDbContext> dbContextFactory,            
        IInformationSecurityEventsLogger informationSecurityEventsLogger,
        ILogger<Cache> logger)
    {
        _configuration = configuration;
        _dbContextFactory = dbContextFactory;
        _informationSecurityEventsLogger = informationSecurityEventsLogger;
        _logger = logger;
    }

    #endregion

    #region public functions
    
    public DbCache DbCache { get; set; } = new DbCache();        

    public Dictionary<string, ImmutableTimeSeriesCache> GrafanaCache { get; } = new();

    public SemaphoreSlim GrafanaCache_SyncRoot { get; } = new(1);

    //public Dictionary<string, object> MainProcess_LargeObjectsCache { get; } = new();

    //public SemaphoreSlim MainProcess_LargeObjectsCache_SyncRoot { get; } = new(1);

    /// <summary>
    ///     Returns true if succeeded.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="roles"></param>
    /// <param name="sourceAddress"></param>
    /// <param name="routeNamespace"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public bool CheckAccess(string user, string[] roles, string sourceAddress, string routeNamespace, HttpRequest request)
    {
        string pathString;
        try
        {
            pathString = request.Path.Value!.Substring(routeNamespace.Length + 1);
        }
        catch
        {
            return false;
        }

        if (String.IsNullOrWhiteSpace(pathString))
            return false;

        bool succeeded = CheckAccessInternal(roles, pathString, request.Method);

        if (!succeeded)
        {
            _logger.LogWarning("API call CheckAccess fail; User {0}; Roles: {1}; SourceAddress: {2}; Request: {3} /{4}",
                user,
                String.Join(',', roles),
                sourceAddress,
                request.Method,
                pathString);
            //_informationSecurityEventsLogger.InformationSecurityEvent(user, sourceAddress, InformationSecurityEventPazCheckCentralServerConstants.ApiCheckAccessFailed_EventId, succeeded, Properties.Resources.ApiCheckAccessFailed_Event, request.Method, pathString);
        }

        //if (!publicApi)
        //{
        //    request.EnableBuffering();
        //    Task.Run(async () =>
        //    {
        //        string bodyString = @"";
        //        if (request.ContentLength > 0)
        //        {
        //            try
        //            {
        //                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
        //                await request.Body.ReadAsync(buffer, 0, buffer.Length);
        //                bodyString = Encoding.UTF8.GetString(buffer);
        //            }
        //            finally
        //            {
        //                request.Body.Position = 0;
        //            }
        //        }                    

        //        _logger.LogInformation("API call; User {0}; Roles: {1}; SourceAddress: {2}; Request: {3} \\{4}; Body: \"{5}\"",
        //            user,
        //            String.Join(',', roles),
        //            sourceAddress,
        //            method,
        //            pathString,
        //            bodyString);

        //        if (properyInfo is not null && Attribute.IsDefined(properyInfo, typeof(InformationSecurityEventEntityAttribute)))
        //        {
        //            //Identifiable<int>? entity;
        //            switch (method)
        //            {
        //                case @"POST":
        //                    //entity = GetEntity(properyInfo, requestStringParts);
        //                    //if (entity is not null)
        //                    _informationSecurityEventsLogger.InformationSecurityEvent(user, sourceAddress, InformationSecurityEventPazCheckCentralServerConstants.EntityPosted_EventId, succeeded, Properties.Resources.EntityPosted_Event, pathString, bodyString);
        //                    break;
        //                case @"PATCH":
        //                    //entity = GetEntity(properyInfo, requestStringParts);
        //                    //if (entity is not null)
        //                    _informationSecurityEventsLogger.InformationSecurityEvent(user, sourceAddress, InformationSecurityEventPazCheckCentralServerConstants.EntityPatched_EventId, succeeded, Properties.Resources.EntityPatched_Event, pathString, bodyString);
        //                    break;
        //                case @"DELETE":
        //                    //entity = GetEntity(properyInfo, requestStringParts);
        //                    //if (entity is not null)
        //                    _informationSecurityEventsLogger.InformationSecurityEvent(user, sourceAddress, InformationSecurityEventPazCheckCentralServerConstants.EntityDeleted_EventId, succeeded, Properties.Resources.EntityDeleted_Event, pathString);
        //                    break;
        //            }
        //        }
        //    });
        //}            

        return succeeded;
    }                     

    public bool CheckAccess(string[] roles, string roleApiFunction_Identifier)
    {
#if !RELEASE_PROD
        if (ConfigurationHelper.GetValue<bool>(_configuration, @"AuthorizationIsDisabled", false))
            return true;
#endif

        _apiFunctionsAndRoles.TryGetValue(roleApiFunction_Identifier, out string[]? apiFunctionRoles);
        return DbCacheHelper.CheckAccess(apiFunctionRoles, roles);
    }

    public string GetEmptyIfAdministrator(string user, HttpContext httpContext)
    {
        if (!String.IsNullOrEmpty(user) && HttpContextHelper.GetUserLowerInvariant(httpContext) == user)
        {
            if (CheckAccess(HttpContextHelper.GetRoles(httpContext), @"Method.SetActiveProjectVersion"))
                user = @""; // Show changes from all users
        }
        return user;
    }        

    public Task<string[]> GetAllRolesAsync()
    {
        return Task.FromResult(_allRoles);
    }                

    public async Task DoWorkAsync(DateTime nowUtc, CancellationToken cancellationToken)
    { 
        if (cancellationToken.IsCancellationRequested || nowUtc < _lastWorkDateTimeUtc + TimeSpan.FromSeconds(30))
            return;
        
        try
        {
            List<Role> roles;
            List<RoleApiFunction> roleApiFunctions;
            using (var readOnlyDbContext = _dbContextFactory.CreateDbContext())
            {
                readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                roles = await readOnlyDbContext.Roles
                    .Include(r => r.RolePermissions.Where(rp => rp.IsAllowed))
                    .ThenInclude(rp => rp.RoleBusinessFunction)
                    .ThenInclude(rp => rp.RoleApiFunctions)
                    .ToListAsync();

                roleApiFunctions = await readOnlyDbContext.RoleApiFunctions
                    .Where(raf => raf.Modifier != @"")
                    .ToListAsync();
            }

            // [RoleApiFunction, [Roles]]
            Dictionary<string, HashSet<string>> apiFunctionsAndRoles = new(StringComparer.InvariantCultureIgnoreCase);
            foreach (Role role in roles)
            {
                foreach (var rolePermission in role.RolePermissions.Where(rp => rp.IsAllowed))
                {
                    foreach (var roleApiFunction in rolePermission.RoleBusinessFunction.RoleApiFunctions)
                    {
                        HashSet<string>? apiFunctionRoles;
                        if (!apiFunctionsAndRoles.TryGetValue(roleApiFunction.Identifier, out apiFunctionRoles))
                        {
                            apiFunctionRoles = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { role.Identifier };
                            apiFunctionsAndRoles.Add(roleApiFunction.Identifier, apiFunctionRoles);
                        }
                        else
                        {
                            apiFunctionRoles.Add(role.Identifier);
                        }
                    }
                }
            }
            HashSet<string> allRoles = new(roles.Select(r => r.Identifier), StringComparer.InvariantCultureIgnoreCase);
            foreach (RoleApiFunction roleApiFunction in roleApiFunctions)
            {
                if (!String.IsNullOrEmpty(roleApiFunction.Modifier))
                    apiFunctionsAndRoles[roleApiFunction.Identifier] = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { roleApiFunction.Modifier };
            }
            _apiFunctionsAndRoles = apiFunctionsAndRoles.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.ToArray()))
                .ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);
            _allRoles = allRoles.ToArray();                
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Critical exception");
        }

        await GrafanaCache_SyncRoot.WaitAsync();
        try
        {
            foreach (var it in GrafanaCache.ToArray())
            {
                if (it.Value.CreateTimeUtc < nowUtc - TimeSpan.FromDays(1))
                    GrafanaCache.Remove(it.Key);
            }                
        }
        finally
        {
            GrafanaCache_SyncRoot.Release();
        }

        _lastWorkDateTimeUtc = nowUtc;                        
    }

    #endregion

    #region private functions        

    private bool CheckAccessInternal(string[] roles, string pathString, string method)
    {
        int i = pathString.IndexOf('?');
        if (i > 0)
            pathString = pathString.Substring(0, i);

        var requestStringParts = pathString.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (String.Equals(requestStringParts[0], @"GrafanaJson", StringComparison.InvariantCultureIgnoreCase) ||
            String.Equals(requestStringParts[0], @"ork", StringComparison.InvariantCultureIgnoreCase) ||
            String.Equals(requestStringParts[0], @"initializers", StringComparison.InvariantCultureIgnoreCase) ||
            String.Equals(requestStringParts[0], @"negotiate", StringComparison.InvariantCultureIgnoreCase))
            return true;

        PazCheckDbContext.EntitiesName_PropertyInfos.TryGetValue(requestStringParts[0], out PropertyInfo? pazCheckDbContext_PropertyInfo);

        string entityName;
        if (pazCheckDbContext_PropertyInfo is not null)
            entityName = pazCheckDbContext_PropertyInfo.PropertyType.GetGenericArguments().First().Name;
        else
            entityName = @"";

        string[]? apiFunctionRoles;

        switch (method)
        {
            case @"GET":
                if (entityName != @"")
                {
                    _apiFunctionsAndRoles.TryGetValue("Entity." + entityName + ".Read", out apiFunctionRoles);
                    if (DbCacheHelper.CheckAccess(apiFunctionRoles, roles))
                        return true;
                    // TODO Add check, that Owner who can Create, also can Read, Delete, Update.
                }
                else if (requestStringParts.Length > 1)
                {
                    _apiFunctionsAndRoles.TryGetValue("Method." + requestStringParts[1], out apiFunctionRoles);
                    if (DbCacheHelper.CheckAccess(apiFunctionRoles, roles))
                        return true;
                }
                break;
            case @"HEAD":
                if (entityName != @"")
                {
                    _apiFunctionsAndRoles.TryGetValue("Entity." + entityName + ".Read", out apiFunctionRoles);
                    if (DbCacheHelper.CheckAccess(apiFunctionRoles, roles))
                        return true;
                }
                break;
            case @"POST":
                if (entityName != @"")
                {
                    _apiFunctionsAndRoles.TryGetValue("Entity." + entityName + ".Create", out apiFunctionRoles);
                    if (DbCacheHelper.CheckAccess(apiFunctionRoles, roles))
                        return true;
                }
                else if (requestStringParts.Length > 1)
                {
                    _apiFunctionsAndRoles.TryGetValue("Method." + requestStringParts[1], out apiFunctionRoles);
                    if (DbCacheHelper.CheckAccess(apiFunctionRoles, roles))
                        return true;
                }
                break;
            case @"PATCH":
                if (entityName != @"")
                {
                    _apiFunctionsAndRoles.TryGetValue("Entity." + entityName + ".Update", out apiFunctionRoles);
                    if (DbCacheHelper.CheckAccess(apiFunctionRoles, roles))
                        return true;
                }
                break;
            case @"DELETE":
                if (entityName != @"")
                {
                    _apiFunctionsAndRoles.TryGetValue("Entity." + entityName + ".Delete", out apiFunctionRoles);
                    if (DbCacheHelper.CheckAccess(apiFunctionRoles, roles))
                        return true;
                }
                break;
        }

        return false;
    }
    
    #endregion

    #region private fields

    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;        
    private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;
    private readonly ILogger _logger;        

    private string[] _allRoles = new string[0];

    /// <summary>
    ///     [RoleApiFunction.Identifier, ApiFunctionRole[]]        
    /// </summary>
    private FrozenDictionary<string, string[]> _apiFunctionsAndRoles = new Dictionary<string, string[]>().ToFrozenDictionary();        

    public DateTime _lastWorkDateTimeUtc = DateTime.UtcNow - TimeSpan.FromSeconds(25);

    #endregion
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