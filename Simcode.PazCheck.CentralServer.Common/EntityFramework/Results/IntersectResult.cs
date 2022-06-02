// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource(PublicName = "intersectresults")]
    public class IntersectResult : Identifiable<int>
    {
        [HasOne(PublicName = "causeresult")]
        public CauseResult CauseResult { get; set; } = null!;
        [HasOne(PublicName = "effectresult")]
        public EffectResult EffectResult { get; set; } = null!;
        [Attr(PublicName = "state")]
        public string State { get; set; } = @"";
        [Attr(PublicName = "triggeredtype")]
        public TriggeredTypes TriggeredType { get; set; } = 0;
        [HasOne(PublicName="diagresult")]
        public DiagResult DiagResult { get; set; } = null!;
    }
}
