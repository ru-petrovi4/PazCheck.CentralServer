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

public class UserEventsLoggerCore : IDisposable
{
    #region construction and destruction       

    public UserEventsLoggerCore(IDbContextFactory<PazCheckDbContext> dbContextFactory)
    {            
        _dbContextFactory = dbContextFactory;            

        _timer = new Timer(OnTimerCallback, null, 5000, 5000);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (Disposed) return;

        if (disposing)
        {
            _timer!.Dispose();

            Flush();
        }

        Disposed = true;
    }

    ~UserEventsLoggerCore()
    {
        Dispose(disposing: false);
    }

    public bool Disposed { get; private set; }

    #endregion

    #region public functions        

    public List<UserEvent> PendingUserEvents
    {
        get 
        {
            lock (_pendingUserEvents)
            {
                return new List<UserEvent>(_pendingUserEvents);                    
            }
        }
    }

    public void AddUserEvent(UserEvent userEvent)
    {
        if (String.IsNullOrEmpty(userEvent.User)) // TEMPCODE
            return;

        lock (_pendingUserEvents)
        {
            _pendingUserEvents.Add(userEvent);
        }
    }

    public void ClearUserEvents(string user)
    {
        if (String.IsNullOrEmpty(user))
            return;            

        if (_dbContextFactory is not null)
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                // SQL injection safe
                dbContext.Database.ExecuteSql($"DELETE FROM \"UserEvents\" WHERE \"User\" = {user}");
            }
    }

    public async Task ClearUserEventsAsync(string user)
    {
        if (String.IsNullOrEmpty(user))
            return;            

        if (_dbContextFactory is not null)
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                // SQL injection safe
                await dbContext.Database.ExecuteSqlAsync($"DELETE FROM \"UserEvents\" WHERE \"User\" = {user}");
            }
    }

    #endregion

    #region private functions

    private void OnTimerCallback(object? state)
    {
        if (Disposed) return;

        Flush();
    }

    private void Flush()
    {
        List<UserEvent> pendingUserEvents;
        lock (_pendingUserEvents)
        {
            pendingUserEvents = _pendingUserEvents;
            _pendingUserEvents = new();
        }

        if (pendingUserEvents.Count > 0)
        {
            using (var dbContext = _dbContextFactory!.CreateDbContext())
            {
                try
                {
                    dbContext.UserEvents.AddRange(pendingUserEvents);
                    dbContext.SaveChanges();
                }
                catch (Exception) 
                { 
                }
            }
        }
    }                

    #endregion

    #region private fields
    
    private readonly IDbContextFactory<PazCheckDbContext>? _dbContextFactory;
    private Timer? _timer;

    private List<UserEvent> _pendingUserEvents = new();

    #endregion
}
