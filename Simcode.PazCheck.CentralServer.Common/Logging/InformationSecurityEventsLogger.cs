using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Properties;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common;

public class InformationSecurityEventsLogger : SszLoggerBase, IInformationSecurityEventsLogger
{
    #region construction and destruction  

    static InformationSecurityEventsLogger()
    {   
        EventIds = typeof(InformationSecurityEventsLogger)
                   .GetFields(BindingFlags.Public | BindingFlags.Static)
                   .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                   .ToDictionary(fi => (int)fi.GetValue(null)!);            
    }

    public InformationSecurityEventsLogger(InformationSecurityEventsLoggerCore informationSecurityEventsLoggerCore)
    {
        _informationSecurityEventsLoggerCore = informationSecurityEventsLoggerCore;
    }

    #endregion

    #region public functions

    /// <summary>
    ///     [EventId, FieldInfo]. Thread-safe.
    /// </summary>
    public static Dictionary<int, FieldInfo> EventIds { get; }

    [PcDisplayName(ResourceStrings.UserLogIn_EventId)]
    public const int UserLogIn_EventId = 0x01;

    [PcDisplayName(ResourceStrings.UserLogOut_EventId)]
    public const int UserLogOut_EventId = 0x02;

    [PcDisplayName(ResourceStrings.RefreshTokenGenerated_EventId)]
    public const int RefreshTokenGenerated_EventId = 0x03;

    [PcDisplayName(ResourceStrings.EntityChanged_EventId)]
    [AllRolesAccessEventId]
    public const int EntityChanged_AllRolesAccessEventId = 0x04;

    [PcDisplayName(ResourceStrings.EntityChanged_EventId)]        
    public const int EntityChanged_EventId = 0x05;

    [PcDisplayName(ResourceStrings.DataImported_EventId)]
    [AllRolesAccessEventId]
    public const int DataImported_AllRolesAccessEventId = 0x06;    

    [PcDisplayName(ResourceStrings.DataImported_EventId)]        
    public const int DataImported_EventId = 0x07;

    [PcDisplayName(ResourceStrings.DataExported_EventId)]
    [AllRolesAccessEventId]
    public const int DataExported_AllRolesAccessEventId = 0x08;

    [PcDisplayName(ResourceStrings.DataExported_EventId)]        
    public const int DataExported_EventId = 0x09;

    [PcDisplayName(ResourceStrings.RoleAttributesChanged_EventId)]
    public const int RoleAttributesChanged_EventId = 0x0A;

    [PcDisplayName(ResourceStrings.RolesChanged_EventId)]
    public const int RolesChanged_EventId = 0x0B;

    [PcDisplayName(ResourceStrings.Access_EventId)]
    public const int Access_EventId = 0x0C;

    [PcDisplayName(ResourceStrings.ComponentStartStop_EventId)]
    public const int ComponentStartStop_EventId = 0x0D;

    [PcDisplayName(ResourceStrings.Connection_EventId)]
    public const int Connection_EventId = 0x0E;

    [PcDisplayName(ResourceStrings.Calculation_EventId)]
    [AllRolesAccessEventId]
    public const int Calculation_AllRolesAccessEventId = 0x0F;

    [PcDisplayName(ResourceStrings.SupervisorApproval_EventId)]
    [AllRolesAccessEventId]
    public const int SupervisorApproval_AllRolesAccessEventId = 0x10;

    [PcDisplayName(ResourceStrings.ConfigurationChange_EventId)]
    public const int ConfigurationChange_EventId = 0x11;

    [PcDisplayName(ResourceStrings.AddonTest_EventId)]
    [AllRolesAccessEventId]
    public const int AddonTest_AllRolesAccessEventId = 0x12;

    [PcDisplayName(ResourceStrings.DataImportError_EventId)]
    [AllRolesAccessEventId]
    public const int DataImportError_AllRolesAccessEventId = 0x13;

    [PcDisplayName(ResourceStrings.WarningRecordsCount_EventId)]
    [AllRolesAccessEventId]
    public const int WarningRecordsCount_AllRolesAccessEventId = 0x14;

    [PcDisplayName(ResourceStrings.DataImported_EventId)]
    [AllRolesAccessEventId]
    public const int DataImportedPreview_AllRolesAccessEventId = 0x15;

    [PcDisplayName(ResourceStrings.EMails_EventId)]
    [AllRolesAccessEventId]
    public const int EMails_AllRolesAccessEventId = 0x16;

    public LogLevel LogLevel { get; set; } = LogLevel.Trace;

    public override bool IsEnabled(LogLevel logLevel)
    {
        if (LogLevel == LogLevel.None) return false;
        return logLevel >= LogLevel;
    }

    public override void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        base.Log(logLevel,
            eventId,
            state,
            exception,
            formatter);

        string user;
        string sourceIpAddress;
        string sourceHost;            
        int severity;
        bool succeeded;
        string eventName;
        string eventSubject;
        string eventObject;
        string eventAdditionalFields;
        string eventDesc;
        lock (SyncRoot)
        {
            user = new Any(TryGetScopeValue(InformationSecurityEventsConstants.UserScopeName)).ValueAsString(false);
            sourceIpAddress = new Any(TryGetScopeValue(InformationSecurityEventsConstants.SourceIpAddressScopeName)).ValueAsString(false);
            sourceHost = new Any(TryGetScopeValue(InformationSecurityEventsConstants.SourceHostScopeName)).ValueAsString(false);                
            severity = new Any(TryGetScopeValue(InformationSecurityEventsConstants.SeverityScopeName)).ValueAsInt32(false);
            succeeded = new Any(TryGetScopeValue(InformationSecurityEventsConstants.SucceededScopeName)).ValueAsBoolean(false);
            eventName = new Any(TryGetScopeValue(InformationSecurityEventsConstants.EventNameScopeName)).ValueAsString(false);
            eventSubject = new Any(TryGetScopeValue(InformationSecurityEventsConstants.EventSubjectScopeName)).ValueAsString(false);
            eventObject = new Any(TryGetScopeValue(InformationSecurityEventsConstants.EventObjectScopeName)).ValueAsString(false);
            eventAdditionalFields = new Any(TryGetScopeValue(InformationSecurityEventsConstants.EventAdditionalFieldsScopeName)).ValueAsString(false);
            eventDesc = GetScopesStringInternal(ExcludeScopeNames);
        }
        try
        {
            eventDesc += formatter(state, exception);
        }
        catch
        {
            eventDesc += "<Invalid message params>";
        }

        bool isAllRolesAccess = EventIds[eventId.Id].GetCustomAttribute<AllRolesAccessEventIdAttribute>() is not null;

        if (!isAllRolesAccess)
        {
            var informationSecurityEvent = new InformationSecurityEvent
            {
                EventTimeUtc = DateTime.UtcNow,
                EventId = eventId.Id,
                EventIdDesc = EventIds[eventId.Id].GetCustomAttribute<PcDisplayNameAttribute>()!.DisplayName,
                Severity = severity,
                SeverityDesc = GetSeverityDesc(severity),
                User = user,
                SourceIpAddress = sourceIpAddress,
                SourceHost = sourceHost,
                EventName = eventName,
                EventSubject = eventSubject,
                EventObject = eventObject,
                EventAdditionalFields = eventAdditionalFields,
                EventDesc = eventDesc,
                Succeeded = succeeded
            };

            _informationSecurityEventsLoggerCore.AddEvent(informationSecurityEvent);
        }
        else
        {
            var allRolesAccessInformationSecurityEventt = new AllRolesAccessInformationSecurityEvent
            {
                EventTimeUtc = DateTime.UtcNow,
                EventId = eventId.Id,
                EventIdDesc = EventIds[eventId.Id].GetCustomAttribute<PcDisplayNameAttribute>()!.DisplayName,
                Severity = severity,
                SeverityDesc = GetSeverityDesc(severity),
                User = user,
                SourceIpAddress = sourceIpAddress,
                SourceHost = sourceHost,
                EventName = eventName,
                EventSubject = eventSubject,
                EventObject = eventObject,
                EventAdditionalFields = eventAdditionalFields,
                EventDesc = eventDesc,
                Succeeded = succeeded
            };

            _informationSecurityEventsLoggerCore.AddEvent(allRolesAccessInformationSecurityEventt);
        }            
    }

    public static string GetSeverityDesc(int severity)
    {
        switch (severity)
        {
            case 1:
            case 2:
            case 3:
                return Properties.Resources.Low_Severity;
            case 4:
            case 5:
            case 6:
                return Properties.Resources.Medium_Severity;
            case 7:
            case 8:
                return Properties.Resources.High_Severity;
            case 9:
            case 10:
                return Properties.Resources.VeryHigh_Severity;
            default:
                return Properties.Resources.Unknown_Severity;
        }
    }         

    #endregion        

    #region private fields

    private static readonly string[] ExcludeScopeNames = new[] {
        InformationSecurityEventsConstants.UserScopeName,
        InformationSecurityEventsConstants.SourceIpAddressScopeName,
        InformationSecurityEventsConstants.SourceHostScopeName,            
        InformationSecurityEventsConstants.SeverityScopeName,
        InformationSecurityEventsConstants.SucceededScopeName,
        InformationSecurityEventsConstants.EventNameScopeName,
        InformationSecurityEventsConstants.EventSubjectScopeName,
        InformationSecurityEventsConstants.EventObjectScopeName,
        InformationSecurityEventsConstants.EventAdditionalFieldsScopeName
    };
    
    private readonly InformationSecurityEventsLoggerCore _informationSecurityEventsLoggerCore;                   

    #endregion
}
