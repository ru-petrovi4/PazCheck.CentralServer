using IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.MicroServices;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public partial class ProjectController : ControllerBase
    {
        #region public functions

        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.Diagnost_Calculate) }
            )]
        [HttpPost("CalculateResults/{projectId}")]
        public Task<IActionResult> CalculateResultsAsync(int projectId, 
            UInt32? projectVersionNum, 
            DateTime beginTimeUtc,
            DateTime endTimeUtc, 
            string addonIdentifier)
        {
            if (projectVersionNum == 0)
                projectVersionNum = null;

            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            var jobId = Guid.NewGuid().ToString();
            if (!_applicationStopping_CancellationToken.IsCancellationRequested)
            {
                _jobsManager.QueueJobIn_AdditionalLongRunningThread(jobId, Properties.Resources.CalculateResults_JobTitle, informationSecurityContext.User,
                    async (cancellationToken, jobProgress) =>
                    {
                        var loggersSet = new LoggersSet(_logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());
                        using var userScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_User, informationSecurityContext.User));
                        await ((UserEventsLogger)loggersSet.UserFriendlyLogger).ClearUserEventsAsync(informationSecurityContext.User);
                        using var scope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((UserEventsLogger.ScopeName_JobId, jobProgress.JobId));

                        CeMatrixRuntimeAddonBase? ceMatrixRuntimeAddon = _addonsManager.CreateInitializedAddonThreadSafe<CeMatrixRuntimeAddonBase>(null, CancellationToken.None);
                        if (ceMatrixRuntimeAddon is null)
                        {
                            _logger.LogError("Invalid addon Type: {0}", "CeMatrixRuntimeAddonBase");
                            await jobProgress.SetJobProgressAsync(null, null, null, StatusCodes.BadInvalidArgument);
                            return;
                        }
                                            
                        string projectTitle = @"";

                        try
                        {
                            await using var dbContext = _dbContextFactory.CreateDbContext();

                            Project project;
                            try
                            {
                                project = dbContext.Projects.Where(p => p.Id == projectId)
                                    .Include(p => p.Unit)
                                    .Single();
                            }
                            catch
                            {
                                _logger.LogError("Invalid projectId: {0}", projectId);
                                await jobProgress.SetJobProgressAsync(null, null, null, StatusCodes.BadInvalidArgument);
                                return;
                            }

                            projectTitle = project.Title;

                            beginTimeUtc = DateTime.SpecifyKind(beginTimeUtc, DateTimeKind.Utc);
                            endTimeUtc = DateTime.SpecifyKind(endTimeUtc, DateTimeKind.Utc);

                            Result? result = await ceMatrixRuntimeAddon.CalculateResultsAsync(_dbContextFactory, 
                                projectId,
                                projectVersionNum,
                                beginTimeUtc,
                                endTimeUtc,
                                informationSecurityContext.User,
                                _cache.DbCache,
                                _jobsManager,
                                cancellationToken, 
                                jobProgress,
                                loggersSet);

                            if (result is not null)
                            {
                                CaseInsensitiveOrderedDictionary<string?> argDicitionary = new()
                                    {
                                        { PazCheckConstants.CriterionName_ResultId, result.Id.ToString() },
                                        { PazCheckConstants.ParamName_UnitIdentifier, project.Unit.Identifier },
                                        { @"AnalysisTime", new Any(result.AlalysisTimeUtc.ToLocalTime()).ValueAsString(true, @"g") },
                                        { @"BeginTime", new Any(result.BeginTimeUtc.ToLocalTime()).ValueAsString(true, @"G") },
                                        { @"EndTime", new Any(result.EndTimeUtc.ToLocalTime()).ValueAsString(true, @"G") },
                                    };

                                foreach (var kvp in result.StatisticsDictionary)
                                {
                                    argDicitionary[kvp.Key] = kvp.Value;
                                }                                

                                var metaParams = dbContext.MetaParams.ToCaseInsensitiveOrderedDictionary(mp => mp.ParamName);                                
                                string arg = NameValueCollectionHelper.GetNameValueCollectionString(argDicitionary);
                                PazCheckDbHelper.AddOrUpdateMetaParam(
                                    dbContext,
                                    metaParams,
                                    paramName: PazCheckDbHelper.GetMetaParamName([ PazCheckConstants.MetaParamNameBase_Diagnost_UnitEventsUserAnalyzed, project.Unit.Identifier ]),
                                    paramValue: Guid.NewGuid().ToString(),
                                    paramType: @"",
                                    isTemp: true, // Deleted periodically
                                    group: @"",
                                    method: @"",
                                    hasArg: true,
                                    excludeConnectionIds: @"",
                                    arg: arg);

                                await dbContext.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, @"CalculateResults error. projectId: {0}", projectId);
                        }
                        finally
                        {
                            _informationSecurityEventsLogger.InformationSecurityEvent(informationSecurityContext.User,
                                    informationSecurityContext.SourceIpAddress,
                                    informationSecurityContext.SourceHost,
                                    InformationSecurityEventsLogger.Calculation_AllRolesAccessEventId,
                                    3,
                                    StatusCodes.IsGood(jobProgress.StatusCode),
                                    Properties.Resources.DiagnostCalculation_Event,
                                    informationSecurityContext.User,
                                    Common.Properties.Resources.Project + @": " + projectTitle,
                                    NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                        {
                                            (@"Action", @"DiagnostAnalysis"),
                                            (@"ProjectId", new Any(projectId).ValueAsString(false)),
                                            (@"BeginTimeUtc", new Any(beginTimeUtc).ValueAsString(false)),
                                            (@"EndTimeUtc", new Any(endTimeUtc).ValueAsString(false)),
                                        }),
                                    Properties.Resources.DiagnostCalculation_EventDesc, projectTitle, beginTimeUtc, endTimeUtc);
                        }
                    });
            }

            return Task.FromResult<IActionResult>(Ok(new { JobId = jobId }));
        }

        #endregion        
    }
}
