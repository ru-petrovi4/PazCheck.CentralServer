// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource(PublicName = "diagresults")]
    public class DiagResult : Identifiable<int>
    {
        [Attr(PublicName = "name")]
        public string Name { get; set; } = @"";

        public int ResultId { get; set; }

        [HasOne(PublicName="result")]
        public Result Result { get; set; } = null!;

        [HasMany(PublicName="causeresults")]
        public List<CauseResult> CauseResults { get; set; } = new();

        [HasMany(PublicName="effectresults")]
        [InverseProperty("DiagResult")]
        public List<EffectResult> EffectResults { get; set; } = new();

        [HasMany(PublicName = "intersectresults")]
        public List<IntersectResult> IntersectResults { get; set; } = new();
    }
}
