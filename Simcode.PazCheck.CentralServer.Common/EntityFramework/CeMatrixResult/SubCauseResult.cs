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
        [Attr(PublicName = "num")]
        public int Num { get; set; } = 0;

        /// <summary>
        ///     Имя тега
        /// </summary>
        [Attr(PublicName = "name")]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     Положение в котором он срабатыает
        /// </summary>
        [Attr(PublicName = "state")]
        public string TagConditionString { get; set; } = @"";

        [Attr(PublicName = "descr")]
        public string CustomFieldValues { get; set; } = @"";

        [Attr(PublicName = "triggeredtime")]
        public DateTime? TriggeredTime { get; set; }

        [HasOne(PublicName = "causeresult")]
        public CauseResult CauseResult { get; set; } = null!;        
    }
}
