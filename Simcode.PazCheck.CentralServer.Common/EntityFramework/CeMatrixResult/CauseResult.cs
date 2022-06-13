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
    public class CauseResult : Identifiable<int>
    {
        [Attr]
        public int Num { get; set; } = 0;

        [HasMany]
        public List<SubCauseResult> SubCauseResults { get; set; } = new();

        [Attr]
        public DateTime? TriggeredTimeUtc { get; set; }

        [HasOne]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;        
    }
}
