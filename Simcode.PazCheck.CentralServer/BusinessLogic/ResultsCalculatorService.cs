using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Simcode.PazCheck.Common;

namespace Simcode.PazCheck.CentralServer.BusinessLogic
{
    //public class ResultsCalculatorService
    //{
    //    private readonly PazCheckDbContext _context;
    //    private readonly ILogger _logger;

    //    public ResultsCalculatorService(PazCheckDbContext context, ILogger<ResultsCalculatorService> logger)
    //    {
    //        _context = context;
    //        _logger = logger;
    //    }

    //    public async Task CalculateResultsAsync(int logId, DateTime startTime, DateTime endTime,
    //        CancellationToken cancellationToken, IJobProgress jobProgress)
    //    {
    //        Log log = await _context.Logs
    //            .Include(l => l.Unit)
    //            .ThenInclude(u => u.ActiveProject)
    //            .ThenInclude(p => p!.Diagrams)
    //            .Include(l =>
    //                l.Logevents.Where(e => e.EventTime >= startTime && e.EventTime <= endTime)
    //                .OrderBy(e => e.EventTime)
    //            )
    //            .FirstAsync(l => l.Id == logId, cancellationToken: cancellationToken);

    //        var logTimeInterval = new LogTimeInterval
    //        {
    //            LogEventsCollection = log.Logevents.ToList()
    //            //LogEventsCollection = log.Logevents
    //            //    .Where(e => e.EventTime >= startTime && e.EventTime <= endTime)
    //            //    .OrderBy(e => e.EventTime)
    //            //    .ToList()
    //        };            

    //        var ceMatricesCollection = new CaseInsensitiveDictionary<CeMatrixModel>();
    //        foreach (var diagram in log.Unit.ActiveProject!.Diagrams) //ToDo: При начале расчета убедиться, что существует активный проект у юнита
    //        {
    //            CeMatrixModel? ceMatrixModel = CeMatrixHelper.CreateCeMatrixTemp(_context, diagram, _logger);
    //            if (ceMatrixModel != null)
    //                ceMatricesCollection.Add(ceMatrixModel.Name, ceMatrixModel);
    //        }

    //        CaseInsensitiveDictionary<CeMatrixRuntime> ceMatrixRuntimesCollection =
    //            CeMatrixHelper.CalculateCeMatrices(ceMatricesCollection, logTimeInterval, log.Unit.ActiveProject, _logger);

    //        var result = new Result
    //        {
    //            Name = log.Name,
    //            Descr = log.Name,
    //            LogFile = log.Name,
    //            ProjectName = log.Unit.ActiveProject!.Name,
    //            Start = startTime,
    //            End = endTime,
    //            Unit = log.Unit,
    //        };
    //        await CeMatrixHelper.SaveCeMatrixRuntimesToDb(result, ceMatrixRuntimesCollection);
    //        _context.Results.Add(result);
    //        await _context.SaveChangesAsync();

    //        await jobProgress.ReportAsync(100, null, null, true);
    //    }        
    //}
}
