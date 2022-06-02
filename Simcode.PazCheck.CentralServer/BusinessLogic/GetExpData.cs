// Copyright (c) 2021
// All rights reserved by Simcode

using System;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.BusinessLogic
{
    public class GetExpData
    {
        public string Id { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public TimeSpan DeltaTime { get; set; }
        public int unitId { get; set; }
    }
}
