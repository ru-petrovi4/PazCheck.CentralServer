// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable

using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class ResultEvent : Identifiable<int>
    {
        /// <summary>
        ///     Порядок
        /// </summary>
        [Attr]
        public int Num { get; set; } = 0;

        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     0 - cause, 1 - effect
        /// </summary>
        [Attr]
        public int Type { get; set; } = 0;

        /// <summary>
        ///     TagCondition_Identifier[=TagCondition_Value]
        /// </summary>
        [Attr]
        public string ConditionString { get; set; } = @"";

        [Attr]
        public TriggeredTypes TriggeredType { get; set; } = 0;

        [Attr]
        public DateTime TriggeredTimeUtc { get; set; } = DateTime.UtcNow;        

        [HasOne]
        public Result Result { get; set; } = null!;
    }
}
