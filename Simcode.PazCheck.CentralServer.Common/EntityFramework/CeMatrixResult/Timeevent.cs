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
        /// <summary>
        ///     Порядок
        /// </summary>
        [Attr(PublicName = "order")]
        public int Num { get; set; } = 0;

        [Attr(PublicName = "name")]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     0 - cause, 1 - effect
        /// </summary>
        [Attr(PublicName = "type")]
        public int Type { get; set; } = 0;

        [Attr(PublicName = "state")]
        public string TagConditionString { get; set; } = @"";

        [Attr(PublicName = "triggeredtype")]
        public TriggeredTypes TriggeredType { get; set; } = 0;

        [Attr(PublicName = "triggeredtime")]
        public DateTime TriggeredTime { get; set; } = DateTime.UtcNow;        

        [HasOne(PublicName = "result")]
        public Result Result { get; set; } = null!;
    }
}
