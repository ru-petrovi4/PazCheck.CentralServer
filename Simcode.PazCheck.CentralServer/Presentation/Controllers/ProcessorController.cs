// Copyright (c) 2021
// All rights reserved by Simcode

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Simcode.PazCheck.CentralServer.BusinessLogic;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    [Route("processor")]
    public class ProcessorController : ControllerBase
    {
        private readonly JobsManager _jobsManager;
        private readonly CancellationToken _cancellationToken;

        public ProcessorController(IHostApplicationLifetime applicationLifetime, JobsManager jobsManager)
        {
            _jobsManager = jobsManager;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        [HttpGet("analyze/{logId}/{startTime}/{endTime}")]
        public async Task<IActionResult> ImportLogAsync(int logId, DateTime startTime, DateTime endTime)
        {
            var guid = Guid.NewGuid().ToString();
            if (!_cancellationToken.IsCancellationRequested)
            {
                //_jobsManager.QueueJob(async token =>
                //    new AnalyzeTaskData {Id = guid, LogId = logId, StartTime = startTime, EndTime = endTime}
                //);
            }

            return Ok(new {guid});
        }

        [HttpGet("getexplog/{unitId}")]
        public async Task<IActionResult> ImportLogAsync( int unitId)
        {
            var guid = Guid.NewGuid().ToString();
            if (!_cancellationToken.IsCancellationRequested)
            {
                //_jobsManager.QueueJob(async token =>
                //    new GetExpData()
                //    {
                //        Id = guid,
                //        unitId = unitId
                //    }
                //);
            }

            return Ok(new {guid});
        }
    }
}
