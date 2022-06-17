// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class IntersectionResult : Identifiable<int>
    {
        [HasOne]
        public CauseResult CauseResult { get; set; } = null!;

        [HasOne]
        public EffectResult EffectResult { get; set; } = null!;

        [Attr]
        public string TagConditionString_SymbolToDisplay { get; set; } = @"";

        [Attr]
        public TriggeredTypes TriggeredType { get; set; } = 0;

        [HasOne]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;
    }
}
