// Copyright (c) 2021
// All rights reserved by Simcode

using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Settings : Identifiable<int>
    {
        [Attr(PublicName="starttime")]
        public DateTime StartTime { get; set; }
        [Attr(PublicName="endtime")]
        public DateTime EndTime { get; set; }
        [Attr(PublicName="deltatime")]
        public TimeSpan DeltaTime { get; set; }
    }
}
