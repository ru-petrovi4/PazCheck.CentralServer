// Copyright (c) 2021
// All rights reserved by Simcode

using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Tagevents : Identifiable<int>
    {
        [Attr(PublicName="eventtime")]
        public DateTime EventTime { get; set; }
        [Attr(PublicName = "value")]
        public string Value { get; set; } = @"";
    }
}
