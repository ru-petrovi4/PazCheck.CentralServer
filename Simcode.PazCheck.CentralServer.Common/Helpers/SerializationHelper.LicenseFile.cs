using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils.Logging;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Linq.Dynamic.Core;
using Ssz.Utils.Addons;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public static partial class SerializationHelper
    {
        public static List<Serialization.LicenseFileInfo> GetLicenseFileInfos(PazCheckDbContext dbContext,
            AddonsManager addonsManager,
            ILoggersSet loggersSet)
        {
            List<Serialization.LicenseFileInfo> result = new();

            foreach (var licenseFile in dbContext.LicenseFiles.Include(Lf => Lf.DbFile).ThenInclude(df => df!.DbFileContent))
            {
                if (licenseFile.DbFile is null || licenseFile.DbFile.DbFileContent is null)
                    continue;

                byte[] licenseFileBytes = Convert.FromBase64String(licenseFile.DbFile.DbFileContent.FileBytes_Base64);

                for (int i = 0; i < licenseFileBytes.Length; i++)
                {
                    licenseFileBytes[i] = (byte)(licenseFileBytes[i] ^ 123);
                }

                string licenseFileString = Encoding.UTF8.GetString(licenseFileBytes);

                const string marker = @"qwertyuiop";
                int i0 = licenseFileString.IndexOf(marker);
                int i1 = licenseFileString.IndexOf(marker, i0 + 1);
                if (i0 > -1 && i1 > i0)
                {
                    Serialization.LicenseFileInfo? licenseFileInfo = null;
                    try
                    {
                        licenseFileInfo = JsonSerializer.Deserialize(
                                licenseFileString.Substring(i0 + marker.Length, i1 - i0 - marker.Length), Simcode.PazCheck.CentralServer.Common.Serialization.SourceGenerationContext.Default.LicenseFileInfo);
                    }
                    catch (Exception ex)
                    {
                        using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Ssz.Utils.Properties.Resources.FileNameScopeName, licenseFile.DbFile.OriginalFileName));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(ex, Properties.Resources.Deserialization_Error + "; " + ex.Message);
                    }
                    if (licenseFileInfo is not null)
                    {
                        licenseFileInfo.LicenseFileId = licenseFile.Id;
                        result.Add(licenseFileInfo);
                    }
                }                
            }

            return result;
        }

        /// <summary>
        ///     Saves changes to dbContext.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <param name="user"></param>
        /// <param name="dbContext"></param>
        /// <param name="loggersSet"></param>
        private static async Task ImportLicenseFileAsync(Stream stream, string fileName, string user, PazCheckDbContext dbContext, ILoggersSet loggersSet)
        {
            var dbFile = await PazCheckDbHelper.GetOrCreateDbFile(dbContext, stream, fileName);

            var licenseFile = new LicenseFile
            {
                _CreateUser = user,                
                Comments = dbFile.OriginalFileName,
                DbFile = dbFile
            };

            dbContext.LicenseFiles.Add(licenseFile);

            await dbContext.SaveChangesAsync();
        }
    }
}
