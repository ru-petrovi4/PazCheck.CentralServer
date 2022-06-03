
using Microsoft.Extensions.Configuration;
using Ssz.Utils;
using System;
using System.IO;

namespace Simcode.PazCheck.Common
{
    public static class ServerConfigurationHelper
    {
        #region public functions

        /// <summary>
        ///     Returns ProgramDataDirectory full path.
        ///     Expands environmental variables, if any. Can handle relative path in settings.
        ///     Throws, if ProgramDataDirectory is not configured.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static DirectoryInfo GetProgramDataDirectoryInfo(IConfiguration configuration)
        {
            string programDataDirectory = ConfigurationHelper.GetValue<string>(configuration, @"ProgramDataDirectory", @"");
            if (programDataDirectory == @"")
            {
                throw new Exception(@"AppSettings ProgramDataDirectory is empty");
            }

            programDataDirectory = Environment.ExpandEnvironmentVariables(programDataDirectory);
            if (!Path.IsPathRooted(programDataDirectory))
                programDataDirectory = Path.Combine(AppContext.BaseDirectory, programDataDirectory);

            return new DirectoryInfo(programDataDirectory);
        }

        /// <summary>
        ///     Throws, if ProgramDataDirectory is not configured.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetAddonsConfigurationDirectoryFullName(IConfiguration configuration)
        {
            DirectoryInfo programDataDirectoryInfo = GetProgramDataDirectoryInfo(configuration);           
            return Path.Combine(programDataDirectoryInfo.FullName, @"Addons");
        }

        /// <summary>
        ///     Throws, if ProgramDataDirectory is not configured.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static string GetExamplesDirectoryFullName(IConfiguration configuration)
        {
            DirectoryInfo programDataDirectoryInfo = GetProgramDataDirectoryInfo(configuration);
            return Path.Combine(programDataDirectoryInfo.FullName, @"..");
        }

        #endregion        
    }
}