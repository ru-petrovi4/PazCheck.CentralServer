// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class CeMatrixResult : Identifiable<int>
    {
        [Attr]
        public int? CeMatrixId { get; set; }

        [Attr]
        public UInt32? ProjectVersionNum { get; set; }

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

        /// <summary>
        ///     JSON string, name-values collection
        /// </summary>
        public string Statistics { get; set; } = @"";

        [Attr]
        [NotMapped]
        public Dictionary<string, string> StatisticsDictionary
        {
            get
            {
                return JsonFieldsHelper.GetDictionary(Statistics);
            }
            set
            {
                Statistics = JsonFieldsHelper.SetDictionary(value);
            }
        }
    }
}
