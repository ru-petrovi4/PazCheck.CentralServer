using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Примечание матрицы
    /// </summary>
    [Resource]    
    public class CeMatrixComment : VersionedEntityBase, IStringIdentifierEntity
    {
        /// <summary>
        ///     Уникальный внутри мктрицы ПСС идентификатор
        /// </summary>
        [Attr]
        public string Identifier { get; set; } = @"";

        public string IdentifierLower { get; set; } = @"";

        /// <summary>
        ///     Текст примечания      
        /// </summary>
        /// <remarks>
        ///     Возможно со знаками \n
        /// </remarks>
        [Attr]
        public string Value { get; set; } = @"";

        public int CeMatrixId { get; set; }

        /// <summary>
        ///     Родительская матрица ПСС
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(CeMatrixId))]
        public CeMatrix CeMatrix { get; set; } = null!;

        public override ProjectVersionedEntityBase? TryGetParentProjectVersionedEntity() => CeMatrix;
        public override bool HasParentProjectVersionedEntity() => true;
        public override Type GetParentProjectVersionedEntity_PropertyType() => typeof(CeMatrix);
        public override int GetParentProjectVersionedEntity_Id() => CeMatrixId;
    }
}
