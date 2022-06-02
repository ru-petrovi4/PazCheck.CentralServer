// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource(PublicName = "effectresults")]
    public class EffectResult : Identifiable<int>
    {
        [Attr(PublicName = "name")]
        public string Name { get; set; } = @""; //Имя тега

        [Attr(PublicName = "descr")]
        public string Descr { get; set; } = @""; //Некая описательная часть

        [Attr(PublicName = "state")]
        public string State { get; set; } = @""; //Положение в котором он срабатыает

        [Attr(PublicName = "triggeredtype")]
        public TriggeredTypes TriggeredType { get; set; } = 0;

        [Attr(PublicName = "triggeredtime")]
        public DateTime? TriggeredTimeUtc { get; set; }

        [Attr(PublicName = "maxdelayms")]
        public UInt64 MaxDelayMs { get; set; } = 0;

        [HasOne(PublicName="diagresult")]
        public DiagResult DiagResult { get; set; } = null!;

        [HasOne(PublicName="firstresult")]
        public DiagResult FirstResult  { get; set; } = null!;

        [Attr(PublicName = "num")]
        public int Num { get; set; } = 0;
    }
}
