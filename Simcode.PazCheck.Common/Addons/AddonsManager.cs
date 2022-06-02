using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ssz.Utils;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Simcode.PazCheck.Common
{
    /// <summary>
    ///     Only GetExportedValues is thread-safe.
    /// </summary>
    public class AddonsManager
    {
        #region construction and destruction

        public AddonsManager(ILogger<AddonsManager> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            Logger = logger;
            Configuration = configuration;
            ServiceProvider = serviceProvider;
        }

        #endregion

        #region public functions

        public EventWaitHandle IsInitializedEventWaitHandle { get; } = new ManualResetEvent(false);

        /// <summary>
        ///     Available addons dlls cached until ResetAvailableAddonsCache()
        /// </summary>
        /// <param name="userFriendlyLogger"></param>
        public void Initialize(IUserFriendlyLogger? userFriendlyLogger)
        {
            UserFriendlyLogger = userFriendlyLogger;

            string addonsConfigurationDirectoryFullName = ServerConfigurationHelper.GetAddonsConfigurationDirectoryFullName(Configuration);
            var addonsConfigurationDirectoryInfo = new DirectoryInfo(addonsConfigurationDirectoryFullName);
            if (!addonsConfigurationDirectoryInfo.Exists)
            {
                Logger.LogInformation(@"<ProgramDataDirectory>\Addons directory does not exist");
                IsInitializedEventWaitHandle.Set();
                return;
            }

            var fullNames = Directory.EnumerateDirectories(addonsConfigurationDirectoryInfo.FullName, "*", SearchOption.TopDirectoryOnly).ToArray();
            if (fullNames.Length == 0)
            {
                Logger.LogInformation(@"ProgramDataDirectory Addons directory does not contains addons config");
                IsInitializedEventWaitHandle.Set();
                return;
            }
            
            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();
            AddonBase[] availableAddons = GetAvailableAddonsCache();
            if (availableAddons.Length == 0)
            {
                Logger.LogInformation(@"No available addons dlls");
                IsInitializedEventWaitHandle.Set();
                return;
            }

            //watch.Stop();
            //Logger.Critical("GetAvailableAddonsCache() time = " + watch.ElapsedMilliseconds + " ms");           

            var desiredAvailableAddons = new HashSet<AddonBase>(AddonsEqualityComparer.Default);
            var catalog = new AggregateCatalog();
            foreach (var addonConfigurationDirectoryFullName in fullNames)
            {
                var addonNameAndInstance = Path.GetFileName(addonConfigurationDirectoryFullName);
                if (addonNameAndInstance is null)
                    continue;
                string addonName;
                string instanceName;
                var parts = addonNameAndInstance.Split(".");
                if (parts.Length == 2)
                {
                    addonName = parts[0];
                    instanceName = parts[1];
                }
                else
                {
                    addonName = addonNameAndInstance;
                    instanceName = "";
                }
                var desiredAvailableAddon =
                    availableAddons.FirstOrDefault(
                        p => String.Equals(p.Name, addonName, StringComparison.InvariantCultureIgnoreCase));
                if (desiredAvailableAddon is not null)
                {
                    bool added = desiredAvailableAddons.Add(desiredAvailableAddon);
                    if (added)
                    {
                        var addonFileInfo =
                            GetAssemblyFileInfo(desiredAvailableAddon.GetType().Assembly);

                        if (addonFileInfo is not null)
                            catalog.Catalogs.Add(new DirectoryCatalog(addonFileInfo.DirectoryName ?? "",
                                addonFileInfo.Name));
                    }                    
                }
            }

            lock (ContainerSyncRoot)
            {
                _container = new CompositionContainer(catalog);
            }

            IsInitializedEventWaitHandle.Set();
        }

        public void Close()
        {
            IsInitializedEventWaitHandle.Reset();

            lock (ContainerSyncRoot)
            {
                _container = null;
            }
        }

        /// <summary>
        ///     Thread-safe
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetExportedValues<T>()
        {
            var result = new List<T>();
            lock (ContainerSyncRoot)
            {
                if (_container is not null)
                    try
                    {
                        result.AddRange(_container.GetExportedValues<T>());
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, @"_container.GetExportedValues<T> Error.");
                    }
            }
            return result;
        }

        /// <summary>
        ///     Thread-safe
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        public T[] GetInitializedAddons<T>(IDispatcher? dispatcher, Func<T, bool>? additionalCheck = null) 
            where T : AddonBase
        {
            var availableAddonsEnumerable = GetExportedValues<AddonBase>().OfType<T>().OrderBy(a => a.IsDummy); // Dummy addons last
            T[] availableAddons;
            if (additionalCheck != null)
            {
                availableAddons = availableAddonsEnumerable.Where(additionalCheck).ToArray();
            }
            else
            {
                availableAddons = availableAddonsEnumerable.ToArray();
            }                
            if (availableAddons.Length == 0)
                return new T[0];

            string addonsConfigurationDirectoryFullName = ServerConfigurationHelper.GetAddonsConfigurationDirectoryFullName(Configuration);
            var addonsConfigurationDirectoryInfo = new DirectoryInfo(addonsConfigurationDirectoryFullName);
            if (!addonsConfigurationDirectoryInfo.Exists)
                return new T[0];

            var fullNames = Directory.EnumerateDirectories(addonsConfigurationDirectoryInfo.FullName, "*", SearchOption.TopDirectoryOnly).ToArray();
            if (fullNames.Length == 0)
                return new T[0];

            var result = new List<T>();

            foreach (var addonConfigurationDirectoryFullName in fullNames)
            {
                var addonNameAndInstance = Path.GetFileName(addonConfigurationDirectoryFullName);
                if (addonNameAndInstance is null)
                    continue;
                string addonName;
                string instanceName;
                var parts = addonNameAndInstance.Split(".");
                if (parts.Length == 2)
                {
                    addonName = parts[0];
                    instanceName = parts[1];
                }
                else
                {
                    addonName = addonNameAndInstance;
                    instanceName = "";
                }
                var desiredAvailableAddon =
                    availableAddons.FirstOrDefault(
                        p => String.Equals(p.Name, addonName, StringComparison.InvariantCultureIgnoreCase));
                if (desiredAvailableAddon is not null)
                {
                    var addon = (T)Activator.CreateInstance(desiredAvailableAddon.GetType())!;
                    addon.Initialize(Logger, Configuration, ServiceProvider, UserFriendlyLogger, dispatcher, instanceName);
                    result.Add(addon);
                }
            }

            return result.ToArray();
        }

        public void ResetAvailableAddonsCache()
        {
            _availableAddons = null;
        }        

        #endregion

        #region private functions    

        private ILogger Logger { get; }

        private IUserFriendlyLogger? UserFriendlyLogger { get; set; }

        private IConfiguration Configuration { get; }

        private IServiceProvider ServiceProvider { get; }

        private AddonBase[] GetAvailableAddonsCache()
        {
            if (_availableAddons is null)
                _availableAddons = GetAvailableAddonsUnconditionally();
            return _availableAddons;
        }

        private AddonBase[] GetAvailableAddonsUnconditionally()
        {
            var exeDirectory = AppContext.BaseDirectory;

            if (exeDirectory is null) return new AddonBase[0];

            var addonsFileInfos = new List<FileInfo>();

            const string addonsSearchPattern = @"Simcode.PazCheck.Addons.*.dll";            
            addonsFileInfos.AddRange(
                Directory.GetFiles(exeDirectory, addonsSearchPattern, SearchOption.TopDirectoryOnly)                    
                    .Select(fn => new FileInfo(fn)));
            try
            {
                string addonsDirerctoryFullname = Path.Combine(exeDirectory, @"Addons");
                if (Directory.Exists(addonsDirerctoryFullname))
                    addonsFileInfos.AddRange(
                        Directory.GetFiles(addonsDirerctoryFullname, addonsSearchPattern, SearchOption.AllDirectories)
                            .Select(fn => new FileInfo(fn)));
            }
            catch
            {
            }

            var availableAddonsList = new List<AddonBase>();
            foreach (FileInfo addonFileInfo in addonsFileInfos)
            {
                var availableAddon = TryGetAddon(addonFileInfo);
                if (availableAddon is not null) availableAddonsList.Add(availableAddon);
            }

            var availableAddonsDictionary = new Dictionary<Guid, AddonBase>();
            var addedAddonsWithDuplicates = new Dictionary<AddonBase, List<string>>();
            foreach (AddonBase addon in availableAddonsList)
                if (!availableAddonsDictionary.TryGetValue(addon.Guid, out var addedAddon))
                {
                    availableAddonsDictionary.Add(addon.Guid, addon);
                }
                else
                {                    
                    var found = false;

                    //Find out if we have this plug in our duplicates list already.
                    foreach (var p in addedAddonsWithDuplicates.Keys)
                        if (p.Guid == addedAddon.Guid)
                            found = true;
                    if (!found)
                    {
                        List<string> duplicateFiles = new();
                        addedAddonsWithDuplicates.Add(addedAddon, duplicateFiles);

                        foreach (AddonBase p in availableAddonsList)
                            if (p.Guid == addedAddon.Guid && !ReferenceEquals(p, addedAddon))
                                //Only include the duplicates.  Do not include the original addon that is being
                                //"properly" loaded up.
                                duplicateFiles.Add(Path.GetDirectoryName(p.DllFileFullName) ?? "");
                    }
                }

//#if !DEBUG
//            if (addedAddonsWithDuplicates.Count > 0)
//            {
//                StringBuilder message = new StringBuilder(byte.MaxValue);
//                message.AppendLine(Properties.Resources.DuplicateAddonsMessage + ":");
//                foreach (var key in addedAddonsWithDuplicates.Keys)
//                {
//                    message.Append(key.Name);
//                    message.Append(" (");
//                    message.Append(Path.GetFileName(key.DllFileFullName));
//                    message.AppendLine(")");

//                    message.Append("    using   - ");
//                    message.AppendLine(Path.GetDirectoryName(key.DllFileFullName));
//                    message.Append("    ignored - ");
//                    foreach (string ignored in addedAddonsWithDuplicates[key])
//                    {
//                        message.AppendLine(ignored);
//                    }
//                }
//                MessageBoxHelper.ShowWarning(message.ToString());
//            }
//#endif

            return availableAddonsDictionary.Values.ToArray();
        }

        private AddonBase? TryGetAddon(FileInfo dllFileInfo)
        {
            if (!dllFileInfo.Exists) return null;

            try
            {
                var catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new DirectoryCatalog(dllFileInfo.DirectoryName ?? "", dllFileInfo.Name));
                using (var container = new CompositionContainer(catalog))
                {
                    var addon = container.GetExportedValues<AddonBase>().FirstOrDefault();
                    if (addon is not null)
                    {
                        addon.DllFileFullName = dllFileInfo.FullName;
                        return addon;
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        private FileInfo? GetAssemblyFileInfo(Assembly assembly)
        {
            if (assembly.IsDynamic) return null;
            //string codeBase = assembly.Location;
            //var uri = new UriBuilder(codeBase);
            //string path = Uri.UnescapeDataString(uri.Path);
            //if (path.StartsWith(@"/")) path = @"//" + uri.Host + path;
            return new FileInfo(assembly.Location);
        }

        #endregion

        #region private fields

        private readonly object ContainerSyncRoot = new();

        private CompositionContainer? _container;

        private AddonBase[]? _availableAddons;

        //private AddonBase[]? _desiredAvailableAddons; // Thread unsafe

        #endregion

        private class AddonsEqualityComparer : EqualityComparer<AddonBase>
        {
            #region public functions

            public static new readonly IEqualityComparer<AddonBase> Default = new AddonsEqualityComparer();

            public override bool Equals(AddonBase? x, AddonBase? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
                return x.Guid == y.Guid;
            }

            public override int GetHashCode(AddonBase obj)
            {
                return obj.Guid.GetHashCode();
            }

            #endregion
        }
    }
}