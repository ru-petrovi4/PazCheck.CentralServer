using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public static partial class PazCheckDbHelper
    {
        #region public functions     

        public static void SetPostgreCryptoPassword(string newPassword, IServiceProvider serviceProvider, IConfiguration configuration, ILoggersSet loggersSet)
        {
            string postgreCryptoPasswordFileName = ConfigurationHelper.GetValue_ValidatedFileName(configuration, PazCheckConstants.ConfigurationKey_PostgreCryptoPasswordFileName, @"", false, true, loggersSet);
            try
            {
                if (!String.IsNullOrEmpty(postgreCryptoPasswordFileName))
                {
                    string oldPassword = GetPostgreCryptoPassword(configuration, loggersSet);

                    var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<PazCheckDbContext>>();

                    using var dbContext = dbContextFactory.CreateDbContext();

                    // SQL injection safe
                    dbContext.Database.ExecuteSql($"UPDATE \"CryptoEntities\" SET \"ValueEncrypted\" = pgp_sym_encrypt_bytea(pgp_sym_decrypt_bytea(\"ValueEncrypted\", {oldPassword}), {newPassword})");

                    Stream stream = File.OpenWrite(postgreCryptoPasswordFileName);

                    using (var writer = new StreamWriter(stream, new UTF8Encoding(true)))
                    {
                        writer.Write(Convert.ToBase64String(new UTF8Encoding(false).GetBytes(newPassword)));
                    }

                    loggersSet.LoggerAndUserFriendlyLogger.LogInformation(Properties.Resources.PasswordSuccesfullyChanged);
                }
            }
            catch (Exception ex)
            {
                using var fileNameScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Ssz.Utils.Properties.Resources.FileNameScopeName, postgreCryptoPasswordFileName));
                loggersSet.LoggerAndUserFriendlyLogger.LogError(ex, "SetEncryptionPassword(...) Exception.");
            }
        }

        /// <summary>
        ///     Returns password from file from config key PostgreCryptoPasswordFileName or default password.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="loggersSet"></param>
        /// <returns></returns>
        public static string GetPostgreCryptoPassword(IConfiguration configuration, ILoggersSet loggersSet)
        {
            string postgreCryptoPasswordFileName = ConfigurationHelper.GetValue_ValidatedFileName(configuration, PazCheckConstants.ConfigurationKey_PostgreCryptoPasswordFileName, @"", true, false, loggersSet);
            try
            {
                if (!String.IsNullOrEmpty(postgreCryptoPasswordFileName))
                {
                    Stream stream = File.OpenRead(postgreCryptoPasswordFileName);

                    using var fileNameScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((CentralServer.Common.Properties.Resources.Scope_FileName, postgreCryptoPasswordFileName));

                    using (var reader = CharsetDetectorHelper.GetStreamReader(stream, Encoding.UTF8, loggersSet))
                    {
                        return Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
                    }
                }
            }
            catch (Exception ex)
            {
                using var fileNameScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Ssz.Utils.Properties.Resources.FileNameScopeName, postgreCryptoPasswordFileName));
                loggersSet.LoggerAndUserFriendlyLogger.LogError(ex, Properties.Resources.Error_FileInvalidFormat);
            }            
            return DefaultEncryptionPassword;
        }

        public static async Task SetPostgreCryptoEntityEntityValueAsync(IDbContextFactory<PazCheckDbContext> dbContextFactory, string identifier, string valueToEncrypt, string comment, string encryptionPassword)
        {
            await using var dbContext = dbContextFactory.CreateDbContext();
            
            byte[] valueToEnctyptBytes = new UTF8Encoding(false).GetBytes(valueToEncrypt);

            string identifierLower = identifier.ToLowerInvariant();
            CryptoEntity? cryptoEntity = dbContext.CryptoEntities.FirstOrDefault(ce => ce.IdentifierLower == identifierLower);
            if (cryptoEntity is null)
            {
                cryptoEntity = new();
                dbContext.CryptoEntities.Add(cryptoEntity);
            }
            cryptoEntity.Identifier = identifier;
            cryptoEntity.ValueHash_Base64 = Convert.ToBase64String(SHA512.HashData(valueToEnctyptBytes));
            cryptoEntity.ValueEncrypted = new byte[0];
            cryptoEntity.Comment = comment ?? @"";
                                
            await dbContext.SaveChangesAsync();

            await dbContext.Database.ExecuteSqlAsync($"UPDATE \"CryptoEntities\" SET \"ValueEncrypted\" = pgp_sym_encrypt_bytea({valueToEnctyptBytes}, {encryptionPassword}) WHERE \"Identifier\"={identifier}");            
        }

        public static string? GetPostgreCryptoEntityValue(PazCheckDbContext dbContext, string identifier, string encryptionPassword)
        {
            // SQL injection safe
            var cryptoEntity = dbContext.CryptoEntities.FromSql($"SELECT \"Id\", \"Identifier\", \"IdentifierLower\", pgp_sym_decrypt_bytea(\"ValueEncrypted\", {encryptionPassword}) AS \"ValueEncrypted\", \"ValueHash_Base64\", \"Comment\" FROM \"CryptoEntities\" WHERE \"IdentifierLower\"={identifier.ToLowerInvariant()}").SingleOrDefault(); // No ; at the end, otherwise error
            if (cryptoEntity is not null)
            {
                return Encoding.UTF8.GetString(cryptoEntity.ValueEncrypted);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region private fields

        private const string DefaultEncryptionPassword = @"3CEA310E-7482-40CC-8505-0147267EDDE5";

        #endregion
    }
}
