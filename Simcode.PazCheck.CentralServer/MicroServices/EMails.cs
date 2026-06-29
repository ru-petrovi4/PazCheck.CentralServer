using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using Simcode.PazCheck.CentralServer.MicroServices;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Simcode.PazCheck.CentralServer.Common.Serialization;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Scriban;
using Ssz.Utils.DataAccess;
using System.Text;
using Ssz.Utils.YamlDotNet.RepresentationModel;
using Ssz.Utils.Yaml;
using Microsoft.Extensions.Configuration;
using Ssz.Utils.YamlDotNet.Serialization;
using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Simcode.PazCheck.CentralServer.MicroServices;

public class EMails
{
    #region construction and destruction

    public EMails(
        IMainServerWorker mainServerWorker,
        IConfiguration configuration,            
        Reports reports,
        IHostApplicationLifetime applicationLifetime,
        IInformationSecurityEventsLogger informationSecurityEventsLogger,
        ILogger<EMails> logger)
    {
        _mainServerWorker = mainServerWorker;
        _configuration = configuration;                  
        _reports = reports;
        _informationSecurityEventsLogger = informationSecurityEventsLogger;
        Logger = logger;
        _applicationStopping_CancellationToken = applicationLifetime.ApplicationStopping;
    }

    #endregion

    #region public functions                        

    public ILogger Logger { get; }

    public void Initialize(CsvDb csvDb, CaseInsensitiveOrderedDictionary<string?> addonOptionsSubstituted)
    {
        _csvDb = csvDb;            
        _addonOptionsSubstituted = addonOptionsSubstituted;            
    }        

    public void Close()
    {
        lock (_userNotificationSubscriptionsCollection)
        {
            foreach (var kvp in _userNotificationSubscriptionsCollection)
            {
                _reports.Unsubscribe(kvp.Key);
            }            
        }
    }

    /// <summary>
    ///     IsMainProcess or not (in case of Test)
    /// </summary>
    public void Load_MailList_FileName(ILoggersSet? loggersSet = null)
    {
        if (loggersSet is null)
            loggersSet = new LoggersSet(Logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());

        var mailList_FileFullName = Path.Combine(_csvDb.CsvDbDirectoryInfo!.FullName, EMailsAddon.MailList_FileName);
        using var fileNameScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((CentralServer.Common.Properties.Resources.Scope_FileName, EMailsAddon.MailList_FileName));

        try
        {                
            if (!File.Exists(mailList_FileFullName))
            {
                loggersSet.UserFriendlyLogger.LogError(Common.Properties.Resources.Error_FileNotFound);
                return;
            }

            YamlStream yaml = new();
            using (Stream fileStream = File.Open(mailList_FileFullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = CharsetDetectorHelper.GetStreamReader(fileStream, Encoding.UTF8, loggersSet))
                {
                    yaml.Load(reader);
                }
            }

            CaseInsensitiveOrderedDictionary<UserNotificationSubscription> userNotificationSubscriptions = new();

            var parser = new YamlConfigurationStreamParser();
            int documentNum = 0;
            foreach (var yamlDocument in yaml.Documents)
            {
                documentNum += 1;
                using var filePartNameScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_FilePartName, documentNum.ToString()));                    

                var keysAndValues = new CaseInsensitiveOrderedDictionary<string?>(parser.Parse(yamlDocument));
                if (keysAndValues.Count == 0)
                    continue;
                string key = nameof(UserNotificationSubscriptionInfo.SubscriptionId);
                string? value = keysAndValues.TryRemoveValue(key);
                if (String.IsNullOrEmpty(value))
                {
                    using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyRequiredIsMissing);
                    continue;
                }
                string subscriptionId = value;

                key = "Period";
                value = keysAndValues.TryRemoveValue(key);
                if (!String.IsNullOrEmpty(value))
                {
                    TimeSpan periodTimeSpan = new Any(value).ValueAs<TimeSpan>(false);
                    if (periodTimeSpan < TimeSpan.FromSeconds(1))
                    {
                        using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyUnknownValue);
                        continue;
                    }
                }
                string? period = value;

                key = "Subject";
                value = keysAndValues.TryRemoveValue(key);
                if (String.IsNullOrEmpty(value))
                {
                    using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyRequiredIsMissing);
                    continue;
                }
                string subject = value;

                key = "BodyTemplate";
                value = keysAndValues.TryRemoveValue(key);
                if (String.IsNullOrEmpty(value))
                {
                    using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyRequiredIsMissing);
                    continue;
                }
                string bodyTemplate = value;

                const string reportsSubKey = "Reports:";
                Dictionary<string, ReportInfo> reportInfos = new();
                foreach (var kvp in keysAndValues)
                {
                    key = kvp.Key;
                    if (key.StartsWith(reportsSubKey, StringComparison.InvariantCultureIgnoreCase))
                    {
                        int i = key.IndexOf(':', reportsSubKey.Length);
                        if (key.Length <= reportsSubKey.Length || 
                            !Char.IsDigit(key[reportsSubKey.Length]) ||
                            i <= reportsSubKey.Length || 
                            i >= key.Length - 1)
                        {
                            using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyUnknownValue);
                            continue;
                        }    
                        string reportNum = key.Substring(reportsSubKey.Length, i - reportsSubKey.Length);                            
                        if (!reportInfos.TryGetValue(reportNum, out ReportInfo? reportInfo))
                        {
                            reportInfo = new ReportInfo();
                            reportInfos.Add(reportNum, reportInfo);
                        }                            
                    }
                }
                foreach (var kvp in reportInfos)
                {
                    ReportInfo reportInfo = kvp.Value;

                    key = reportsSubKey + kvp.Key + ":AddonIdentifier";
                    value = keysAndValues.TryRemoveValue(key);
                    if (value is null)
                    {
                        using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyRequiredIsMissing);
                        continue;
                    }
                    reportInfo.AddonIdentifier = value;                    

                    key = reportsSubKey + kvp.Key + ":DestinationTypeIdentifier";
                    value = keysAndValues.TryRemoveValue(key);
                    if (value is null)
                    {
                        using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyRequiredIsMissing);
                        continue;
                    }
                    reportInfo.DestinationTypeIdentifier = value;

                    reportInfo.FilterInfo = GetFilterInfo(reportsSubKey + kvp.Key + ":Filter:", keysAndValues, loggersSet);
                }

                UserNotificationSubscriptionInfo userNotificationSubscriptionInfo;
                CaseInsensitiveOrderedDictionary<string?> constants = new();

                key = "SourceEvent:Category";
                value = keysAndValues.TryRemoveValue(key) ?? @"";
                switch (value.ToUpperInvariant())
                {
                    case "TIME":
                        {   
                            key = "SourceEvent:Time";
                            value = keysAndValues.TryRemoveValue(key);
                            if (String.IsNullOrEmpty(value))
                            {
                                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                                loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyRequiredIsMissing);
                                continue;
                            }
                            DateTime dateTime = (new Any(value).ValueAs<DateTime>(false)).ToUniversalTime();
                            if (dateTime < new DateTime(2000, 01, 01))
                            {
                                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                                loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyUnknownValue);
                                continue;
                            }

                            TimePoint_UserNotificationSubscriptionInfo timePoint_UserNotificationSubscriptionInfo = new()
                            {
                                SubscriptionId = subscriptionId,
                                PeriodicEvents_Period = period,
                                ReportInfos = reportInfos.Count > 0 ? reportInfos.Values.ToArray() : null,
                                Constants = constants,
                                EventHandler = NotifyUsers,
                                PeriodicEvents_BeginDateTimeUtc = dateTime
                            };                                

                            userNotificationSubscriptionInfo = timePoint_UserNotificationSubscriptionInfo;
                        }
                        break;
                    case "EVENT":
                        {
                            // Analyzed MetaParam.ParamName before PazCheckCentralServerConstants.MetaParamName_FieldsSeparator
                            key = "SourceEvent:Type";
                            value = keysAndValues.TryRemoveValue(key);
                            if (String.IsNullOrEmpty(value))
                            {
                                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                                loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyRequiredIsMissing);
                                continue;
                            }
                            string sourceEvent_Type = value;
                            //if (!Check_SourceEvent_Type(sourceEvent_Type))
                            //{
                            //    using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                            //    loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyUnknownValue);
                            //    continue;
                            //}

                            EventOccurs_UserNotificationSubscriptionInfo eventOccurs_UserNotificationSubscriptionInfo = new()
                            {
                                SubscriptionId = subscriptionId,
                                PeriodicEvents_Period = period,
                                ReportInfos = reportInfos.Count > 0 ? reportInfos.Values.ToArray() : null,
                                Constants = constants,
                                EventHandler = NotifyUsers,
                                SourceEvent_Type = sourceEvent_Type,
                                SourceEvent_Type_Regex = PazCheckDbHelper.GetRegexOrNull(sourceEvent_Type),
                                SourceEvent_FilterInfo = GetFilterInfo("SourceEvent:Filter:", keysAndValues, loggersSet)
                            };
                            userNotificationSubscriptionInfo = eventOccurs_UserNotificationSubscriptionInfo;
                        }
                        break;
                    case "!EVENT":
                        {
                            // Analyzed MetaParam.ParamName before PazCheckCentralServerConstants.MetaParamName_FieldsSeparator
                            key = "SourceEvent:Type";
                            value = keysAndValues.TryRemoveValue(key);
                            if (String.IsNullOrEmpty(value))
                            {
                                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                                loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyRequiredIsMissing);
                                continue;
                            }
                            string sourceEvent_Type = value;
                            //if (!Check_SourceEvent_Type(sourceEvent_Type))
                            //{
                            //    using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                            //    loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyUnknownValue);
                            //    continue;
                            //}

                            TimeSpan allowedTimeSpan;
                            key = "SourceEvent:AllowedTimeSpan";
                            value = keysAndValues.TryRemoveValue(key);
                            if (String.IsNullOrEmpty(value))
                            {
                                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                                loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyRequiredIsMissing);
                                continue;
                            }
                            else
                            {
                                allowedTimeSpan = new Any(value).ValueAs<TimeSpan>(false);
                                if (allowedTimeSpan < TimeSpan.FromSeconds(1))
                                {
                                    using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyUnknownValue);
                                    continue;
                                }
                            }                                

                            EventDoesNotOccur_UserNotificationSubscriptionInfo eventDoesNotOccur_UserNotificationSubscriptionInfo = new()
                            {
                                SubscriptionId = subscriptionId,
                                PeriodicEvents_Period = period,
                                ReportInfos = reportInfos.Count > 0 ? reportInfos.Values.ToArray() : null,
                                Constants = constants,
                                EventHandler = NotifyUsers,
                                SourceEvent_Type = sourceEvent_Type,
                                SourceEvent_Type_Regex = PazCheckDbHelper.GetRegexOrNull(sourceEvent_Type),
                                SourceEvent_FilterInfo = GetFilterInfo("SourceEvent:Filter:", keysAndValues, loggersSet),
                                SourceEvent_AllowedTimeSpan = allowedTimeSpan
                            };
                            userNotificationSubscriptionInfo = eventDoesNotOccur_UserNotificationSubscriptionInfo;
                        }
                        break;
                    default:
                        {
                            using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyUnknown);
                        }
                        continue;
                }
                
                UserNotificationSubscription userNotificationSubscription = new()
                {
                    UserNotificationSubscriptionInfo = userNotificationSubscriptionInfo,                    
                    Subject = subject,
                    BodyTemplate = bodyTemplate,
                    EMails = new()
                };                

                foreach (var kvp in keysAndValues.ToArray())
                {
                    key = kvp.Key;
                    if (key.StartsWith("EMails:", StringComparison.InvariantCultureIgnoreCase) && !String.IsNullOrEmpty(kvp.Value))
                    {
                        keysAndValues.Remove(kvp.Key);
                        int i = kvp.Value.IndexOf("@");
                        if (i > 0 && i < kvp.Value.Length - 1)
                        {
                            userNotificationSubscription.EMails.Add(kvp.Value);
                        }
                        else
                        {
                            using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyUnknownValue);
                        }
                    }
                    else if (key.StartsWith("Constants:", StringComparison.InvariantCultureIgnoreCase) && !String.IsNullOrEmpty(kvp.Value))
                    {
                        keysAndValues.Remove(kvp.Key);
                        string constantName = kvp.Key.Substring("Constants:".Length);
                        if (constantName.StartsWith(@"%(") && constantName.EndsWith(@")"))
                        {
                            if (constants.ContainsKey(constantName))
                            {
                                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                                loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyDuplicateValue);
                            }
                            else
                            {
                                constants.Add(constantName, kvp.Value);
                            }
                        }
                        else
                        {
                            using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                            loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyUnknownValue);
                        }
                    }
                }                
                if (userNotificationSubscription.EMails.Count == 0)
                {
                    key = @"EMails";
                    using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyRequiredIsMissing);
                    continue;
                }
                if (userNotificationSubscriptions.ContainsKey(userNotificationSubscription.UserNotificationSubscriptionInfo.SubscriptionId))
                {
                    key = nameof(UserNotificationSubscriptionInfo.SubscriptionId);
                    using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyDuplicateValue);
                    continue;
                }
                if (keysAndValues.Count > 0)
                {
                    foreach (var kvp in keysAndValues)
                    {                            
                        using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, kvp.Key));
                        loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyDuplicateValue);
                    }
                    continue;
                }
                userNotificationSubscriptions.Add(userNotificationSubscription.UserNotificationSubscriptionInfo.SubscriptionId, userNotificationSubscription);
            }                

            lock (_userNotificationSubscriptionsCollection)
            {
                foreach (var kvp in _userNotificationSubscriptionsCollection)
                {
                    _reports.Unsubscribe(kvp.Key);
                }
                _userNotificationSubscriptionsCollection = new(userNotificationSubscriptions);
                foreach (var kvp in _userNotificationSubscriptionsCollection)
                {
                    _reports.Subscribe(kvp.Value.UserNotificationSubscriptionInfo);
                }
            }                
        }
        catch (Exception ex)
        {
            loggersSet.UserFriendlyLogger.LogError(ex, Common.Properties.Resources.Error_SeeApplicationLogFilesForDetails);
            loggersSet.Logger.LogError(ex, "EMailsAddon Load_MailList_FileName() error.");
        }
    }        

    public async Task NotifyUsers(object? sender, UserNotificationEventArgs args)
    {
        ILoggersSet loggersSet;
        if (args.LoggersSet is not null)
            loggersSet = args.LoggersSet;
        else
            loggersSet = new LoggersSet(Logger, _mainServerWorker.ServiceProvider.GetRequiredService<UserEventsLogger>());

        UserNotificationSubscription? userNotificationSubscription;
        lock (_userNotificationSubscriptionsCollection)
        {
            _userNotificationSubscriptionsCollection.TryGetValue(args.SubscriptionId, out userNotificationSubscription);
        }
        if (userNotificationSubscription is null)
        {
            Logger.LogError("NotifyUsers. Invalid SubscriptionId.");
            return;
        }

        bool succeeded = false;

        try
        {
            string? eMailFrom_Name = _addonOptionsSubstituted.TryGetValue(EMailsAddon.EMailFrom_Name_OptionName);
            if (String.IsNullOrEmpty(eMailFrom_Name))
            {
                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_AddonOption, EMailsAddon.EMailFrom_Name_OptionName));
                loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Common.Properties.Resources.InvalidAddonOptionValue_Empty);
                throw new OperationCanceledException(Common.Properties.Resources.SeeUserEventsLog);
            }
            string? eMailFrom_Address = _addonOptionsSubstituted.TryGetValue(EMailsAddon.EMailFrom_Address_OptionName);
            if (String.IsNullOrEmpty(eMailFrom_Address))
            {
                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_AddonOption, EMailsAddon.EMailFrom_Address_OptionName));
                loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Common.Properties.Resources.InvalidAddonOptionValue_Empty);
                throw new OperationCanceledException(Common.Properties.Resources.SeeUserEventsLog);
            }
            string? smtpHost = _addonOptionsSubstituted.TryGetValue(EMailsAddon.SmtpHost_OptionName);
            if (String.IsNullOrEmpty(smtpHost))
            {
                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_AddonOption, EMailsAddon.SmtpHost_OptionName));
                loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Common.Properties.Resources.InvalidAddonOptionValue_Empty);
                throw new OperationCanceledException(Common.Properties.Resources.SeeUserEventsLog);
            }
            string? smtpPort = _addonOptionsSubstituted.TryGetValue(EMailsAddon.SmtpPort_OptionName);
            if (String.IsNullOrEmpty(smtpPort))
            {
                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((CentralServer.Common.Properties.Resources.Scope_AddonOption, EMailsAddon.SmtpPort_OptionName));
                loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Common.Properties.Resources.InvalidAddonOptionValue_Empty);
                throw new OperationCanceledException(Common.Properties.Resources.SeeUserEventsLog);
            }
            string? smtpUserName = _addonOptionsSubstituted.TryGetValue(EMailsAddon.SmtpUserName_OptionName);
            if (String.IsNullOrEmpty(smtpUserName))
            {
                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((CentralServer.Common.Properties.Resources.Scope_AddonOption, EMailsAddon.SmtpUserName_OptionName));
                loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Common.Properties.Resources.InvalidAddonOptionValue_Empty);
                throw new OperationCanceledException(Common.Properties.Resources.SeeUserEventsLog);
            }
            string? smtpPassword = _addonOptionsSubstituted.TryGetValue(EMailsAddon.SmtpPassword_OptionName);
            if (String.IsNullOrEmpty(smtpPassword))
            {
                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((CentralServer.Common.Properties.Resources.Scope_AddonOption, EMailsAddon.SmtpPassword_OptionName));
                loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Common.Properties.Resources.InvalidAddonOptionValue_Empty);
                throw new OperationCanceledException(Common.Properties.Resources.SeeUserEventsLog);
            }
            string? smtpParams = _addonOptionsSubstituted.TryGetValue(EMailsAddon.SmtpParams_OptionName);
            bool dangerousAcceptAnyServerCertificatete = new Any(_addonOptionsSubstituted.TryGetValue(EMailsAddon.DangerousAcceptAnyServerCertificate_OptionName)).ValueAsBoolean(false);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(eMailFrom_Name, eMailFrom_Address));

            if (!args.IsTestMode)
            {
                foreach (string email in userNotificationSubscription.EMails)
                {                        
                    if (!String.IsNullOrEmpty(email))
                        message.To.Add(new MailboxAddress(email, email));
                }
            }
            else
            {
                if (String.IsNullOrEmpty(args.TestOptions))
                {                        
                    loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Properties.Resources.InvalidTestOptionsValue_Empty);
                    throw new OperationCanceledException(Common.Properties.Resources.SeeUserEventsLog);
                }

                foreach (string? email in CsvHelper.ParseCsvLine(",", args.TestOptions))
                {
                    if (!String.IsNullOrEmpty(email))
                        message.To.Add(new MailboxAddress(email, email));
                }
            }

            CaseInsensitiveOrderedDictionary<string?> updatedConstants = new CaseInsensitiveOrderedDictionary<string?>(args.Constants);
            foreach (var kvp in _addonOptionsSubstituted)
            {
                if (kvp.Key.StartsWith("%(Smtp", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                updatedConstants[kvp.Key] = kvp.Value;
            }                              

            Func<string, IterationInfo, string>? getConstantValue = (c, iterationInfo) =>
            {
                var v = updatedConstants.TryGetValue(c);
                if (!String.IsNullOrEmpty(v))
                    return v;

                using var constantScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((CentralServer.Common.Properties.Resources.Scope_Constant, c));
                if (args.IsTestMode)
                    loggersSet.LoggerAndUserFriendlyLogger.LogInformation(Common.Properties.Resources.Error_IdentifierUnknown);
                else
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_IdentifierUnknown);

                return c;
            };

            message.Subject = SszQueryHelper.ComputeValueOfSszQueries(userNotificationSubscription.Subject, getConstantValue);

            string htmlFileName = SszQueryHelper.ComputeValueOfSszQueries(userNotificationSubscription.BodyTemplate, getConstantValue);
            string htmlFileFullName = Path.Combine(_csvDb.CsvDbDirectoryInfo?.FullName ?? @"", Path.GetFileName(htmlFileName) ?? @"");
            if (!File.Exists(htmlFileFullName))
            {
                using var fileNameScope = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((CentralServer.Common.Properties.Resources.Scope_FileName, htmlFileName));
                loggersSet.LoggerAndUserFriendlyLogger.LogWarning(Common.Properties.Resources.Error_FileNotFound);
                throw new OperationCanceledException(Common.Properties.Resources.SeeUserEventsLog);
            }

            var multipart = new Multipart("mixed");

            Template template;
            using (StreamReader streamReader = CharsetDetectorHelper.GetStreamReader(htmlFileFullName, Encoding.UTF8, loggersSet))
            {
                template = Template.Parse(await streamReader.ReadToEndAsync());
            }
            var model = new CaseInsensitiveOrderedDictionary<string>(updatedConstants.Select(kvp => KeyValuePair.Create(PrepareConstantKeyForModel(kvp.Key), PrepareConstantValueForModel(kvp.Value))));
            try
            {
                var body = new TextPart("html")
                {
                    Text = template.Render(model)
                };
                multipart.Add(body);
            }
            catch (Exception ex)
            {
                using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_FileName, htmlFileName));
                loggersSet.LoggerAndUserFriendlyLogger.LogError(ex, Common.Properties.Resources.Error_HtmlTemplate);
                throw new OperationCanceledException(Common.Properties.Resources.SeeUserEventsLog);
            }

            if (args.IsTestMode && args.DebugInfo.Count > 0)
            {
                var debugInfo_Body = new TextPart("html")
                {
                    Text = "---<br><strong>" + Properties.Resources.DebugInfoHeader + "</strong><br>" + String.Join("<br>", args.DebugInfo)
                };
                multipart.Add(debugInfo_Body);
            }                
            
            foreach (var attachment in args.Attachments)
            {
                multipart.Add(new MimePart("application", "octet-stream")
                {
                    Content = new MimeContent(new MemoryStream(attachment.Item2)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = attachment.Item1
                });
            }
            message.Body = multipart;

            using (var client = new SmtpClient())
            {
                if (dangerousAcceptAnyServerCertificatete)
                    client.ServerCertificateValidationCallback =
                        (object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true;
                var smtpParamsDictionary = NameValueCollectionHelper.Parse(smtpParams);
                bool useSsl = ConfigurationHelper.GetValue<bool>(smtpParamsDictionary, @"useSsl", false);
                try
                {
                    await client.ConnectAsync(smtpHost, new Any(smtpPort).ValueAsInt32(false), useSsl);
                }
                catch (Exception ex)
                {
                    throw new Exception($"SmtpClient.ConnectAsync(smtpHost: {smtpHost}, smtpPort: {new Any(smtpPort).ValueAsInt32(false)}, useSsl: {useSsl}) Error.", ex);
                }                    

                try
                {
                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate(smtpUserName, smtpPassword);
                }
                catch (Exception ex)
                {
                    throw new Exception($"SmtpClient.Authenticate(smtpUserName: {smtpUserName}, smtpPassword: ******) Error.", ex);
                }                    

                try
                {
                    client.Send(message);
                }
                catch (Exception ex)
                {
                    throw new Exception("SmtpClient.Send(message) Error.", ex);
                }                    

                try
                {
                    client.Disconnect(true);
                }
                catch (Exception ex)
                {
                    throw new Exception("SmtpClient.Disconnect(true) Error.", ex);
                }                    
            }

            succeeded = true;
            
        }
        catch (OperationCanceledException)
        {
            throw;
        }                        
        catch (Exception ex)
        {
            loggersSet.LoggerAndUserFriendlyLogger.LogError(ex, Common.Properties.Resources.Error_SendEMail);
            throw new OperationCanceledException(Common.Properties.Resources.SeeUserEventsLog);
        }
        finally
        {
            string user = Common.Properties.Resources.ObjectSystem;
            _informationSecurityEventsLogger.InformationSecurityEvent(user,
                                    @"127.0.0.1",
                                    @"localhost",
                                    InformationSecurityEventsLogger.EMails_AllRolesAccessEventId,
                                    3,
                                    succeeded,
                                    Properties.Resources.EMails_Sending_Event,
                                    user,
                                    user,
                                    NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                        {
                                            (@"SubscriptionId", args.SubscriptionId),
                                            (@"IsTestMode", new Any(args.IsTestMode).ValueAsString(false)),
                                            (@"EMails", String.Join(";", userNotificationSubscription.EMails))
                                        }),
                                    Properties.Resources.EMails_Sending_Event);
        }
    }

    public async Task TestAsync(string testOptions, ILoggersSet loggersSet)
    {
        Load_MailList_FileName(loggersSet);

        await _reports.TestAsync(testOptions, loggersSet);
    }        

    #endregion

    #region private functions        

    private string PrepareConstantKeyForModel(string? c)
    {
        if (String.IsNullOrEmpty(c))
            return @"";

        c = c.Substring(2, c.Length - 3);            

        return c;
    }

    private string PrepareConstantValueForModel(string? c)
    {
        if (String.IsNullOrEmpty(c))
            return @"";
        
        c = c.Replace("\n", "<br>", StringComparison.InvariantCultureIgnoreCase);

        return c;
    }

    private bool Check_SourceEvent_Type(string sourceEvent_Type)
    {
        return PazCheckConstants.SourceEvent_TypesCollection.Any(t => String.Equals(t, sourceEvent_Type, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    ///     subKey with ':' at the end.
    /// </summary>
    /// <param name="subKey"></param>
    /// <param name="keysAndValues"></param>
    /// <param name="loggersSet"></param>
    /// <returns></returns>
    private List<CaseInsensitiveOrderedDictionary<List<string>>> GetFilterInfo(string subKey, CaseInsensitiveOrderedDictionary<string?> keysAndValues, ILoggersSet loggersSet)
    {
        CaseInsensitiveOrderedDictionary<CaseInsensitiveOrderedDictionary<List<string>>> filterInfo = new();            
        foreach (var kvp in keysAndValues.ToArray())
        {
            string key = kvp.Key;
            if (key.StartsWith(subKey, StringComparison.InvariantCultureIgnoreCase))
            {
                keysAndValues.Remove(key);

                int filterKeyBeginIndex = key.IndexOf(':', subKey.Length) + 1;
                if (key.Length <= subKey.Length ||
                    !Char.IsDigit(key[subKey.Length]) ||
                    filterKeyBeginIndex == -1 ||
                    filterKeyBeginIndex >= key.Length)
                {
                    using var s = loggersSet.LoggerAndUserFriendlyLogger.BeginScope((Common.Properties.Resources.Scope_KeyName, key));
                    loggersSet.LoggerAndUserFriendlyLogger.LogError(Common.Properties.Resources.Error_KeyUnknownValue);
                    continue;
                }
                string filterNum = key.Substring(subKey.Length, filterKeyBeginIndex - subKey.Length - 1);
                if (!filterInfo.TryGetValue(filterNum, out CaseInsensitiveOrderedDictionary<List<string>>? partFilterInfo))
                {
                    partFilterInfo = new();
                    filterInfo.Add(filterNum, partFilterInfo);
                }
                string partFilterKey = key.Substring(filterKeyBeginIndex);                    
                if (!partFilterInfo.TryGetValue(partFilterKey, out var valuesList))
                {
                    valuesList = new List<string>();
                    partFilterInfo.Add(partFilterKey, valuesList);
                }
                valuesList.Add(kvp.Value ?? @"");
            }
        }
        return filterInfo.Values.ToList();
    }

    #endregion

    #region private fields

    private readonly IMainServerWorker _mainServerWorker;
    private readonly IConfiguration _configuration;             
    private readonly Reports _reports;        
    private readonly IInformationSecurityEventsLogger _informationSecurityEventsLogger;        
    private readonly CancellationToken _applicationStopping_CancellationToken;
    private DateTime _lastDoWorkDateTimeUtc;

    private CsvDb _csvDb = null!;
    private CaseInsensitiveOrderedDictionary<string?> _addonOptionsSubstituted = null!;

    /// <summary>
    ///     [SubscriptionId, UserNotificationSubscription]
    /// </summary>
    private Dictionary<string, UserNotificationSubscription> _userNotificationSubscriptionsCollection = new();

    #endregion        

    private class UserNotificationSubscription
    {
        public UserNotificationSubscriptionInfo UserNotificationSubscriptionInfo { get; init; } = null!;        

        public string Subject { get; init; } = null!;

        public string BodyTemplate { get; init; } = null!;

        public List<string> EMails { get; init; } = null!;
    }
}



//        public async Task DoWorkAsync(DateTime nowUtc, CancellationToken cancellationToken, CaseInsensitiveOrderedDictionary<string?> optionsSubstitutedThreadSafe)
//        {
//            int updatePeriodSeconds = new Any(optionsSubstitutedThreadSafe.TryGetValue(EMailsAddon.UpdatePeriodSeconds_OptionName)).ValueAsInt32(false);

//            if (updatePeriodSeconds <= 1)
//                return;

//            if (nowUtc < _lastDoWorkDateTimeUtc + TimeSpan.FromSeconds(updatePeriodSeconds))
//                return;
//            _lastDoWorkDateTimeUtc = nowUtc;

//            /*
//            var message = new MimeMessage();
//            message.From.Add(new MailboxAddress("Joey Tribbiani", "joey@friends.com"));
//            message.To.Add(new MailboxAddress("Mrs. Chanandler Bong", "chandler@friends.com"));
//            message.Subject = "How you doin'?";

//            message.Body = new TextPart("plain")
//            {
//                Text = @"Hey Chandler,

//I just wanted to let you know that Monica and I were going to go play some paintball, you in?

//-- Joey"
//            };

//            using (var client = new SmtpClient())
//            {
//                await client.ConnectAsync("smtp.friends.com", 587, false);

//                // Note: only needed if the SMTP server requires authentication
//                client.Authenticate("joey", "password");

//                client.Send(message);
//                client.Disconnect(true);
//            }*/
//        }