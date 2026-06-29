using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Ssz.Utils;
using Ssz.Utils.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Simcode.PazCheck.CentralServer.MicroServices
{
    public class ConfigurationProcessor : IConfigurationProcessor
    {
        #region construction and destruction

        public ConfigurationProcessor(IDbContextFactory<PazCheckDbContext> dbContextFactory,           
            ILogger<ConfigurationProcessor> logger,
            IConfiguration configuration)
        {            
            _dbContextFactory = dbContextFactory;            
            _logger = logger;
            _configuration = configuration;            
        }

        #endregion

        #region public functions

        [return: NotNullIfNotNull("value")]
        public string? ProcessValue(string? value)
        {
            return SszQueryHelper.ComputeValueOfSszQueries(value, GetConstantValue);
        }

        #endregion

        #region private functions

        private string GetConstantValue(string constant, IterationInfo iterationInfo)
        {
            if (constant.StartsWith(@"%(yml:", StringComparison.InvariantCultureIgnoreCase))
            {
                var length = @"%(yml:".Length;
                return _configuration.GetValue<string>(@"Encypted:" + constant.Substring(length, constant.Length - length - 1)) ?? @"";
            }
            else if (constant.StartsWith(@"%(db:", StringComparison.InvariantCultureIgnoreCase))
            {
                var length = @"%(db:".Length;
                string encryptionPassword = PazCheckDbHelper.GetPostgreCryptoPassword(_configuration, new LoggersSet(_logger, null));
                string identifier = constant.Substring(length, constant.Length - length - 1);

                try
                {
                    using (var dbContext = _dbContextFactory.CreateDbContext())
                    {
                        return PazCheckDbHelper.GetPostgreCryptoEntityValue(dbContext, identifier, encryptionPassword) ?? @"";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "GetCryptoEntityValue error: " + identifier);
                }
            }
            using var constantScope = _logger.BeginScope((CentralServer.Common.Properties.Resources.Scope_Constant, constant));
            _logger.LogError(Common.Properties.Resources.Error_IdentifierUnknown);
            return constant;
        }

        #endregion

        #region private fields
        
        private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;                
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        #endregion
    }
}
