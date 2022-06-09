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

        [Attr(PublicName = "triggeredtype")]
        public TriggeredTypes TriggeredType { get; set; } = 0;

        [Attr(PublicName = "triggeredtime")]
        public DateTime? TriggeredTimeUtc { get; set; }

        [Attr(PublicName = "maxdelayms")]
        public UInt64 MaxDelayMs { get; set; } = 0;

        [HasOne(PublicName="diagresult")]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;        
    }
}
