// Copyright (c) 2021
// All rights reserved by Simcode

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Simcode.PazCheck.CentralServer.BusinessLogic;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    //[Route("processor")]
    //public class ProcessorController : ControllerBase
    //{
    //    private readonly JobsManager _jobsManager;
    //    private readonly CancellationToken _cancellationToken;
    //    private readonly AddonsManager _addonsManager;

    //    public ProcessorController(IHostApplicationLifetime applicationLifetime, JobsManager jobsManager, AddonsManager addonsManager)
    //    {
    //        _jobsManager = jobsManager;
    //        _cancellationToken = applicationLifetime.ApplicationStopping;
    //        _addonsManager = addonsManager;
    //    }

    //    [HttpGet("analyze/{logId}/{startTime}/{endTime}")]
    //    public async Task<IActionResult> ImportLogAsync(int logId, DateTime startTime, DateTime endTime)
    //    {
    //        var jobId = Guid.NewGuid().ToString();
    //        if (!_cancellationToken.IsCancellationRequested)
    //        {
    //            _jobsManager.QueueJob(jobId, "Import Log",
    //                        async (cancellationToken, jobProgress) =>
    //                        {
    //                            var ceMatrixRuntimeAddonBase = _addonsManager.Addons.OfType<CeMatrixRuntimeAddonBase>().OrderBy(a => a.IsDummy).FirstOrDefault();
    //                            if (ceMatrixRuntimeAddonBase is null)
    //                            {
    //                                return;
    //                            }

    //                            using (var dbContext = new PazCheckDbContext())
    //                            {                                    
    //                                await ceMatrixRuntimeAddonBase.CalculateResultsAsync(dbContext, logId, startTime, endTime, cancellationToken, jobProgress);
    //                            }
    //                        });                
    //        }

    //        return Ok(new { jobId });
    //    }

    //    [HttpGet("getexplog/{unitId}")]
    //    public async Task<IActionResult> ImportLogAsync( int unitId)
    //    {
    //        var guid = Guid.NewGuid().ToString();
    //        if (!_cancellationToken.IsCancellationRequested)
    //        {
    //            //_jobsManager.QueueJob(async token =>
    //            //    new GetExpData()
    //            //    {
    //            //        Id = guid,
    //            //        unitId = unitId
    //            //    }
    //            //);
    //        }

    //        return Ok(new {guid});
    //    }
    //}
}
