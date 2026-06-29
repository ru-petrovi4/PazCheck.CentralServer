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

public class InformationSecurityEventsLoggerCore : IDisposable
{
    #region construction and destruction          

    public InformationSecurityEventsLoggerCore(IDbContextFactory<PazCheckDbContext> dbContextFactory)
    {            
        _dbContextFactory = dbContextFactory;            

        _timer = new Timer(OnTimerCallback, null, 1000, 1000);
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
            _timer.Dispose();

            Flush();
        }

        Disposed = true;
    }

    ~InformationSecurityEventsLoggerCore()
    {
        Dispose(disposing: false);
    }

    public bool Disposed { get; private set; }

    #endregion

    #region public functions        

    public void AddEvent(InformationSecurityEvent informationSecurityEvent)
    {
        lock (_pendingInformationSecurityEvents)
        {
            _pendingInformationSecurityEvents.Add(informationSecurityEvent);
        }
    }

    public void AddEvent(AllRolesAccessInformationSecurityEvent allRolesAccessInformationSecurityEvent)
    {
        lock (_pendingAllRolesAccessInformationSecurityEvents)
        {
            _pendingAllRolesAccessInformationSecurityEvents.Add(allRolesAccessInformationSecurityEvent);
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
        List<InformationSecurityEvent> pendingInformationSecurityEvents;
        lock (_pendingInformationSecurityEvents)
        {
            pendingInformationSecurityEvents = _pendingInformationSecurityEvents;
            _pendingInformationSecurityEvents = new();
        }            

        List<AllRolesAccessInformationSecurityEvent> pendingAllRolesAccessInformationSecurityEvents;
        lock (_pendingAllRolesAccessInformationSecurityEvents)
        {
            pendingAllRolesAccessInformationSecurityEvents = _pendingAllRolesAccessInformationSecurityEvents;
            _pendingAllRolesAccessInformationSecurityEvents = new();
        }

        if (pendingInformationSecurityEvents.Count > 0)
        {
            try
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    dbContext.InformationSecurityEvents.AddRange(pendingInformationSecurityEvents);
                    dbContext.SaveChanges();
                }
            }
            catch
            {
            }
        }

        if (pendingAllRolesAccessInformationSecurityEvents.Count > 0)
        {
            try
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    dbContext.AllRolesAccessInformationSecurityEvents.AddRange(pendingAllRolesAccessInformationSecurityEvents);
                    dbContext.SaveChanges();
                }
            }
            catch
            {
            }
        }
    }               

    #endregion

    #region private fields
    
    private readonly IDbContextFactory<PazCheckDbContext> _dbContextFactory;
    private Timer _timer;

    private List<InformationSecurityEvent> _pendingInformationSecurityEvents = new();
    private List<AllRolesAccessInformationSecurityEvent> _pendingAllRolesAccessInformationSecurityEvents = new();                

    #endregion
}
