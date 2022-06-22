// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class SubCauseResult : Identifiable<int>
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
        public string TagConditionString_SymbolToDisplay { get; set; } = @"";

        [Attr]
        public string CustomFieldValues { get; set; } = @"";

        [Attr]
        public DateTime? TriggeredTimeUtc { get; set; }

        [Attr]
        public CauseResult CauseResult { get; set; } = null!;        
    }
}
