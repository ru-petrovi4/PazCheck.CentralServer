// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource(PublicName = "subcauseresults")]
    public class SubCauseResult : Identifiable<int>
    {
        [Attr(PublicName = "name")]
        public string Name { get; set; } = @""; //Имя тега
        [Attr(PublicName = "descr")]
        public string Descr { get; set; } = @""; //Некая описательная часть
        [Attr(PublicName = "state")]
        public string State { get; set; } = @""; //Положение в котором он срабатыает
        [Attr(PublicName = "triggeredtime")]
        public DateTime? TriggeredTime { get; set; }
        [HasOne(PublicName = "causeresult")]
        public CauseResult CauseResult { get; set; } = null!;
        [Attr(PublicName = "num")]
        public int Num { get; set; } = 0;
    }
}
