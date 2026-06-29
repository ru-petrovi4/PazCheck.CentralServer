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
        ///     Идентификатор матрицы ПСС
        /// </summary>
        [Attr]
        public string Identifier { get; set; } = @"";

        /// <summary>
        ///     Родительский результат анализа
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(ResultId))]
        public Result Result { get; set; } = null!;

        /// <summary>
        ///     Родительский результат анализа
        /// </summary>
        public int ResultId { get; set; }

        /// <summary>
        ///     Строки
        /// </summary>
        [HasMany]
        [InverseProperty(nameof(RowResult.CeMatrixResult))] // Bacause CellResults exists.
        public List<RowResult> RowResults { get; set; } = new();

        /// <summary>
        ///     Столбцы
        /// </summary>
        [HasMany]
        [InverseProperty(nameof(ColumnResult.CeMatrixResult))] // Bacause IntersectionResults exists.
        public List<ColumnResult> ColumnResults { get; set; } = new();

        /// <summary>
        ///     Ячейки
        /// </summary>
        [HasMany]
        public List<CellResult> CellResults { get; set; } = new();

        /// <summary>
        ///     Свойства матрицы
        /// </summary>
        /// <remarks>
        ///     Url encoded name-values collection.
        /// </remarks>
        public string CeMatrixParams { get; set; } = @"";

        /// <summary>
        ///     Статистика по матрице
        /// </summary>
        [Attr]
        [NotMapped]
        public CaseInsensitiveOrderedDictionary<string?> CeMatrixParamsDictionary
        {
            get
            {
                return NameValueCollectionHelper.Parse(CeMatrixParams);
            }
            set
            {
                CeMatrixParams = NameValueCollectionHelper.GetNameValueCollectionString(value);
            }
        }

        /// <summary>
        ///     Статистика по матрице
        /// </summary>
        /// <remarks>
        ///     Url encoded name-values collection.
        /// </remarks>
        public string CeMatrixComments { get; set; } = @"";

        /// <summary>
        ///     Статистика по матрице
        /// </summary>
        [Attr]
        [NotMapped]
        public CaseInsensitiveOrderedDictionary<string?> CeMatrixCommentsDictionary
        {
            get
            {
                return NameValueCollectionHelper.Parse(CeMatrixComments);
            }
            set
            {
                CeMatrixComments = NameValueCollectionHelper.GetNameValueCollectionString(value);
            }
        }

        /// <summary>
        ///     Статистика по матрице
        /// </summary>
        /// <remarks>
        ///     Url encoded name-values collection.
        /// </remarks>
        public string Statistics { get; set; } = @"";

        /// <summary>
        ///     Статистика по матрице
        /// </summary>
        [Attr]
        [NotMapped]        
        public CaseInsensitiveOrderedDictionary<string?> StatisticsDictionary
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
