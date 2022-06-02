// Copyright (c) 2021
// All rights reserved by Simcode

using System;

namespace Simcode.PazCheck.CentralServer.BusinessLogic
{
    public class AnalyzeTaskData
    {
        public string Id { get; set; }
        public int LogId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
