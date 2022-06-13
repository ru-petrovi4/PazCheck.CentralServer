// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class EffectResult : Identifiable<int>
    {
        [Attr]
        public int Num { get; set; } = 0;

        /// <summary>
        ///     Имя тега
        /// </summary>
        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     Положение в котором он срабатыает
        /// </summary>
        [Attr]
        public string TagConditionString { get; set; } = @"";

        [Attr]
        public string CustomFieldValues { get; set; } = @"";  

        [Attr]
        public TriggeredTypes TriggeredType { get; set; } = 0;

        [Attr]
        public DateTime? TriggeredTimeUtc { get; set; }

        [Attr]
        public UInt64 MaxDelayMs { get; set; } = 0;

        [HasOne]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;        
    }
}
