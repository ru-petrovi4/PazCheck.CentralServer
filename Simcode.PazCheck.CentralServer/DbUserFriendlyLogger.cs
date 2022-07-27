using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils.Logging;
using System;
using System.Linq;

namespace Simcode.PazCheck.CentralServer
{
    public class DbUserFriendlyLogger : SszLoggerBase
    {
        #region public functions

        public override bool IsEnabled(LogLevel logLevel) => true;

        public override void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            string message = @"";
            lock (ScopeStringsStack)
            {
                foreach (var scopeString in ScopeStringsStack.Reverse())
                {
                    message += scopeString + @" -> ";
                }
            }
            message += formatter(state, exception);

            string details = @"";
            Exception? ex = exception;
            while (ex is not null)
            {
                details += "Exception: " + ex.Message + "\n";
                ex = ex.InnerException;
            }

            using (var dbContext = new PazCheckDbContext())
            {
                dbContext.LogUserEvents.Add(new LogUserEvent
                {
                    EventTimeUtc = DateTime.UtcNow,
                    LogLevel = (int)logLevel,
                    Message = message,
                    Details = details,
                });
                dbContext.SaveChanges();
            }
        }

        #endregion
    }
}
