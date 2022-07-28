using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Ssz.Utils;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Результат анализа матрицы ПСС
    /// </summary>
    [Resource]
    public class CeMatrixResult : Identifiable<int>
    {
        /// <summary>
        ///     ID оригинальной проанализированной матрицы
        /// </summary>
        [Attr]
        public int? CeMatrixId { get; set; }

        /// <summary>
        ///     Версия проекта проанализированной матрицы
        /// </summary>
        [Attr]
        public UInt32? ProjectVersionNum { get; set; }

        /// <summary>
        ///     Родительский результат анализа
        /// </summary>
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
        ///     Статистика по матрице. Url encoded name-values collection
        /// </summary>
        public string Statistics { get; set; } = @"";

        [Attr]
        [NotMapped]        
        public Dictionary<string, string?> StatisticsDictionary
        {
            get
            {
                return NameValueCollectionHelper.Parse(Statistics);
            }
            set
            {
                Statistics = NameValueCollectionHelper.GetNameValueCollectionString(value);
            }
        }
    }
}
