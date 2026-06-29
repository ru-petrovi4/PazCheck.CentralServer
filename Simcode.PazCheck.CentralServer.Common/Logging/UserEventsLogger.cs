using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common;

public class UserEventsLogger : SszLoggerBase
{
    #region construction and destruction       

    public UserEventsLogger(UserEventsLoggerCore userEventsLoggerCore)
    {   
        _userEventsLoggerCore = userEventsLoggerCore;
    }

    #endregion

    #region public functions

    public const string ScopeName_JobId = @"JobId";

    public const string ScopeName_User = @"User";    

    public LogLevel LogLevel { get; set; } = LogLevel.Information;

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

        string jobId;
        string user;
        string message;
        string details;
        lock (SyncRoot)
        {
            jobId = new Any(TryGetScopeValue(ScopeName_JobId)).ValueAsString(true);
            user = new Any(TryGetScopeValue(ScopeName_User)).ValueAsString(true);            
            details = NameValueCollectionHelper.GetNameValueCollectionString(GetScopesInternal(ExcludeScopeNames));
        }
        try
        {
            message = formatter(state, exception);
        }
        catch
        {
            message = "<Invalid message params>";
        }
        
        Exception? ex = exception;
        while (ex is not null)
        {
            if (message != @"")
                message += "; ";
            message += "Exception: " + ex.Message + "\n";
            ex = ex.InnerException;
        }

        var userEvent = new UserEvent
        {
            EventTimeUtc = DateTime.UtcNow,
            JobId = jobId,
            User = user,
            LogLevel = (int)logLevel,
            Message = message,
            Details = details,
        };

        _userEventsLoggerCore.AddUserEvent(userEvent);
    }

    public void ClearUserEvents(string user)
    {
        ClearStatistics();

        _userEventsLoggerCore.ClearUserEvents(user);
    }

    public async Task ClearUserEventsAsync(string user)
    {
        ClearStatistics();

        await _userEventsLoggerCore.ClearUserEventsAsync(user);
    }    

    #endregion

    #region private fields

    private static readonly string[] ExcludeScopeNames = new[] { ScopeName_JobId, ScopeName_User };        
    
    private readonly UserEventsLoggerCore _userEventsLoggerCore;    

    #endregion
}
