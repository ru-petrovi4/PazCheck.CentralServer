// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable

using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Timeevent : Identifiable<int>
    {
        [Attr(PublicName = "name")]
        public string Name { get; set; } = @"";
        [Attr(PublicName = "type")]
        public int Type { get; set; } = 0; //0 - cause, 1 - effect
        [Attr(PublicName = "state")]
        public string State { get; set; } = @""; //Положение в котором он срабатыает
        [Attr(PublicName = "triggeredtype")]
        public TriggeredTypes TriggeredType { get; set; } = 0;
        [Attr(PublicName = "triggeredtime")]
        public DateTime TriggeredTime { get; set; } = DateTime.UtcNow;
        [Attr(PublicName = "order")]
        public int Order { get; set; } = 0; // Порядок
        [HasOne(PublicName = "result")]
        public Result Result { get; set; } = null!;
    }
}
