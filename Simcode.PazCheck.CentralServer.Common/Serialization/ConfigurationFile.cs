using IdentityServer4.Validation;
using Ssz.Utils;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public class ConfigurationFile
    {
        #region public functions
        
        public static ConfigurationFile CreateFrom(Ssz.Utils.Addons.ConfigurationFile configurationFile)
        {
            ConfigurationFile newConfigurationFile = new()
            {
                SourcePath = configurationFile.SourcePath,
                SourceId = configurationFile.SourceId,
                SourceIdToDisplay = configurationFile.SourceIdToDisplay,                
                PathRelativeToRootDirectory = configurationFile.InvariantPathRelativeToRootDirectory,
                LastWriteTimeUtc = configurationFile.LastModified.UtcDateTime,
                Length = configurationFile.Length,
                IsDeleted = configurationFile.IsDeleted
            };

            if (configurationFile.FileData is not null)
            {
                string base64String = Convert.ToBase64String(configurationFile.FileData);

                if (configurationFile.Name.EndsWith(@".csv", StringComparison.InvariantCultureIgnoreCase))
                    newConfigurationFile.FileData = "data:application/text/csv;base64," + base64String;
                if (configurationFile.Name.EndsWith(@".yml", StringComparison.InvariantCultureIgnoreCase))
                    newConfigurationFile.FileData = "data:application/text/yaml;base64," + base64String;
                if (configurationFile.Name.EndsWith(@".html", StringComparison.InvariantCultureIgnoreCase) ||
                    configurationFile.Name.EndsWith(@".htm", StringComparison.InvariantCultureIgnoreCase))
                    newConfigurationFile.FileData = "data:application/text/html;base64," + base64String;
                else
                    newConfigurationFile.FileData = "data:application/octet-stream;base64," + base64String;
            }

            return newConfigurationFile;
        }

        public Ssz.Utils.Addons.ConfigurationFile ToConfigurationFile()
        {
            Ssz.Utils.Addons.ConfigurationFile newConfigurationFile = new()
            {
                SourcePath = this.SourcePath,
                SourceId = this.SourceId,
                SourceIdToDisplay = this.SourceIdToDisplay,
                InvariantPathRelativeToRootDirectory = this.PathRelativeToRootDirectory,
                LastModified = this.LastWriteTimeUtc,
                Length = this.Length,
                IsDeleted = this.IsDeleted
            };

            if (FileData is not null)
            {   
                string fileData = FileData;
                int i = fileData.IndexOf(',');
                if (i > -1)
                    fileData = fileData.Substring(i + 1);

                newConfigurationFile.FileData = Convert.FromBase64String(fileData);                
            }

            return newConfigurationFile;
        }

        /// <summary>        
        /// </summary>
        public string Name => PathRelativeToRootDirectory.Substring(PathRelativeToRootDirectory.LastIndexOf('/') + 1);

        /// <summary>        
        ///     Path separator is always '/'. No '/' at the begin, no '/' at the end.
        /// </summary>        
        public string SourcePath { get; set; } = @"";

        /// <summary>        
        ///     Globally-unique service (process) id.
        /// </summary>        
        public string SourceId { get; set; } = @"";

        /// <summary>        
        ///     Globally-unique service (process) id to display.
        /// </summary>        
        public string SourceIdToDisplay { get; set; } = @"";

        /// <summary>        
        ///     Path relative to the root of the Files Store.
        ///     Path separator is always '/'. No '/' at the begin, no '/' at the end.        
        /// </summary>        
        public string PathRelativeToRootDirectory { get; set; } = @"";

        /// <summary>
        ///     FileInfo.LastWriteTimeUtc
        /// </summary>        
        public DateTime LastWriteTimeUtc { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     FileInfo.Length
        /// </summary> 
        public long Length { get; set; }

        /// <summary>
        ///     File content.
        /// </summary>        
        public string? FileData { get; set; }

        /// <summary>
        ///     Whether file must be deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        #endregion
    }
}
