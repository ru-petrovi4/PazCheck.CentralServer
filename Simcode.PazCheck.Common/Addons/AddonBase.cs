using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ssz.Utils;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.Common
{
    public abstract class AddonBase
    {
        #region public functions

        public const string SimcodePazCheckServerVersionConst = @"1";

        public const string OptionsCsvFileName = @"Options.csv";

        public abstract Guid Guid { get; }

        public abstract string Name { get; }

        public string InstanceName { get; private set; } = @"";

        public abstract string Desc { get; }

        public abstract string Version { get; }

        public virtual bool IsDummy => false;

        public abstract string SimcodePazCheckServerVersion { get; }

        public abstract (string, string)[] OptionsInfo { get; }

        public string DllFileFullName { get; set; } = @"";

        public CsvDb CsvDb { get; private set; } = null!;

        public virtual void Initialize(ILogger logger, IConfiguration configuration, IServiceProvider serviceProvider, IUserFriendlyLogger? userFriendlyLogger, IDispatcher? dispatcher, string instanceName)
        {
            Logger = logger;
            UserFriendlyLogger = userFriendlyLogger;
            var loggersList = new List<ILogger>() { Logger };
            if (UserFriendlyLogger is not null)
                loggersList.Add(UserFriendlyLogger);
            WrapperUserFriendlyLogger = new WrapperUserFriendlyLogger(loggersList.ToArray());
            Configuration = configuration;
            ServiceProvider = serviceProvider;
            InstanceName = instanceName;
            Dispatcher = dispatcher;

            var parameters = new List<object>();
            if (userFriendlyLogger != null) parameters.Add(userFriendlyLogger);
            parameters.Add(GetCsvDbDirectoryInfo());
            if (dispatcher != null) parameters.Add(dispatcher);
            CsvDb = ActivatorUtilities.CreateInstance<CsvDb>(ServiceProvider, parameters.ToArray());            
        }

        public virtual void Close()
        {
        }        

        #endregion

        #region protected functions

        /// <summary>
        ///     Has value after Initialize()
        /// </summary>
        protected ILogger Logger { get; private set; } = null!;

        /// <summary>
        ///     May have value after Initialize()
        /// </summary>
        protected IUserFriendlyLogger? UserFriendlyLogger { get; private set; }

        /// <summary>
        ///     Writes to Logger and UserFriendlyLogger, if any.
        ///     Has value after Initialize()
        /// </summary>
        protected WrapperUserFriendlyLogger WrapperUserFriendlyLogger { get; private set; } = null!;

        protected IConfiguration Configuration { get; private set; } = null!;

        protected IServiceProvider ServiceProvider { get; private set; } = null!;

        protected IDispatcher? Dispatcher { get; private set; }

        #endregion

        #region private functions

        private DirectoryInfo GetCsvDbDirectoryInfo()
        {
            string addonsConfigurationDirectoryFullName = ServerConfigurationHelper.GetAddonsConfigurationDirectoryFullName(Configuration);            

            string csvDb = Path.Combine(addonsConfigurationDirectoryFullName, Name + (InstanceName != @"" ? "." + InstanceName : @""));

            Logger.LogDebug("CsvDb: " + csvDb);
            
            return new DirectoryInfo(csvDb);
        }        

        #endregion
    }
}
