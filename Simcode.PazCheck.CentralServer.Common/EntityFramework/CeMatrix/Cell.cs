using System;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Ячейка в матрице ПСС
    /// </summary>
    [Resource]
    public class Cell : VersionedEntityBase
    {
        /// <summary>
        ///     Ссылка на строку
        /// </summary>
        [HasOne]
        public Row Row { get; set; } = null!;
        
        /// <summary>
        ///     Ссылка на столбец
        /// </summary>
        [HasOne]
        public Column Column { get; set; } = null!;

        /// <summary>
        ///     Содержимое ячейки
        /// </summary>        
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     Родительская матрица ПСС
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(CeMatrixId))]
        public CeMatrix CeMatrix { get; set; } = null!;

        /// <summary>
        ///     Родительская матрица ПСС
        /// </summary>
        public int CeMatrixId { get; set; }

        public override ProjectVersionedEntityBase? TryGetParentProjectVersionedEntity() => CeMatrix;
        public override bool HasParentProjectVersionedEntity() => true;
        public override Type GetParentProjectVersionedEntity_PropertyType() => typeof(CeMatrix);
        public override int GetParentProjectVersionedEntity_Id() => CeMatrixId;
    }
}
