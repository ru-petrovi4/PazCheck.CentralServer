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
    ///     ��������� ������� ������� ���
    /// </summary>
    [Resource]
    public class CeMatrixResult : Identifiable<int>
    {
        /// <summary>
        ///     ID ������������ ������������������ �������
        /// </summary>
        [Attr]
        public int? CeMatrixId { get; set; }

        /// <summary>
        ///     ������ ������� ������������������ �������
        /// </summary>
        [Attr]
        public UInt32? ProjectVersionNum { get; set; }

        /// <summary>
        ///     ������������ ��������� �������
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
        ///     ���������� �� �������. Url encoded name-values collection
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
