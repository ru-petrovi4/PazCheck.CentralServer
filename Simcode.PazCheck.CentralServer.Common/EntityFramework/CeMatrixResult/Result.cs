// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Result : Identifiable<int>
    {
        [Attr(PublicName = "name")]
        public string Title { get; set; } = @"";

        [Attr(PublicName = "descr")]
        public string Desc { get; set; } = @"";

        public string Comment { get; set; } = @"";

        [Attr(PublicName="logfile")]
        public string LogFile { get; set; } = @"";

        [Attr(PublicName="projectname")]
        public string ProjectName { get; set; } = @"";

        [Attr(PublicName = "alalyzedate")]
        public DateTime AlalyzeDate { get; set; } = DateTime.UtcNow;

        [Attr(PublicName = "issaved")]
        public bool IsSaved { get; set; } = false;

        [Attr(PublicName = "start")]
        public DateTime Start { get; set; }

        [Attr(PublicName = "end")]
        public DateTime End { get; set; }

        [HasMany(PublicName="diagresults")]
        public List<CeMatrixResult> DiagResults { get; set; } = new();

        [HasOne(PublicName = "unit")]
        public Unit Unit { get; set; } = null!;

        [HasMany(PublicName="timeevents")]
        public List<Timeevent> Timeevents { get; set; } = new();
    }
}
