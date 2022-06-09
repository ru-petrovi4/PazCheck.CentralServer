// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource(PublicName = "causeresults")]
    public class CauseResult : Identifiable<int>
    {
        [Attr(PublicName = "num")]
        public int Num { get; set; } = 0;

        [HasMany(PublicName = "subcauseresults")]
        public List<SubCauseResult> SubCauseResults { get; set; } = new();

        [Attr(PublicName = "triggeredtime")]
        public DateTime? TriggeredTimeUtc { get; set; }

        [HasOne(PublicName="diagresult")]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;        
    }
}
