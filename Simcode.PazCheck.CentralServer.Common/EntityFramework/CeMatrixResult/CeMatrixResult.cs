// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class CeMatrixResult : Identifiable<int>
    {
        [Attr]
        public string Guid { get; set; } = @"";

        [Attr]
        public int Version { get; set; }

        [HasOne]
        public Result Result { get; set; } = null!;

        [HasMany]
        [InverseProperty(nameof(CauseResult.CeMatrixResult))] // Bacause IntersectResults exists.
        public List<CauseResult> CauseResults { get; set; } = new();

        [HasMany]
        [InverseProperty(nameof(EffectResult.CeMatrixResult))] // Bacause IntersectionResults exists.
        public List<EffectResult> EffectResults { get; set; } = new();

        [HasMany]
        public List<IntersectionResult> IntersectionResults { get; set; } = new();

        [HasOne]
        public Project Project { get; set; } = null!;
    }
}
