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
        ///     Is column with debug info
        /// </summary>
        [Attr]
        public bool IsDebug { get; set; }

        /// <summary>
        ///     Имя тега
        /// </summary>
        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     TagCondition_Identifier[=TagCondition_Value]
        /// </summary>
        [Attr]
        public string ConditionString { get; set; } = @"";

        [Attr]
        public string ConditionString_SymbolToDisplay { get; set; } = @"";

        [Attr]
        public string CustomFieldHeader { get; set; } = @"";  

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
