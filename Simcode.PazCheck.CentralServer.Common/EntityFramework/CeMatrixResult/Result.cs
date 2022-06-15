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
    public class Result : Identifiable<int>
    {
        [Attr]
        public string Title { get; set; } = @"";

        [Attr]
        public string Desc { get; set; } = @"";

        [Attr]
        public string Comment { get; set; } = @"";

        [Attr]
        public string LogFile { get; set; } = @"";

        [Attr]
        public string ProjectGuid { get; set; } = @"";

        [Attr]
        public DateTime AlalyzeDate { get; set; } = DateTime.UtcNow;

        [Attr]
        public bool IsSaved { get; set; } = false;

        [Attr]
        public DateTime StartTimeUtc { get; set; }

        [Attr]
        public DateTime EndTimeUtc { get; set; }

        [HasMany]
        public List<CeMatrixResult> CeMatrixResults { get; set; } = new();

        [HasOne]
        public Unit Unit { get; set; } = null!;

        [HasMany]
        public List<Timeevent> Timeevents { get; set; } = new();
    }
}
