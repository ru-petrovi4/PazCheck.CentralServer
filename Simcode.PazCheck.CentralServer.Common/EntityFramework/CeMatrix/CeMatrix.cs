using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Матрица ПСС    
    /// </summary>    
    [Resource]    
    public class CeMatrix : ProjectVersionedEntityBase
    {
        /// <summary>
        ///     Свойства матрицы
        /// </summary>
        [HasMany]
        public List<CeMatrixParam> CeMatrixParams { get; set; } = new();

        /// <summary>
        ///     Ссылки на приложенные файлы
        /// </summary>
        [HasMany]
        public List<CeMatrixDbFileReference> CeMatrixDbFileReferences { get; set; } = new();

        /// <summary>
        ///     Строки матрицы ПСС
        /// </summary>
        /// <remarks>
        ///     Не хранятся в json, хранятся в csv.
        /// </remarks>
        [HasMany]
        [InverseProperty(nameof(Row.CeMatrix))] // Bacause Cell with Row exists.
        public List<Row> Rows { get; set; } = new();

        /// <summary>
        ///     Столбцы матрицы ПСС
        /// </summary>
        /// <remarks>
        ///     Не хранятся в json, хранятся в csv.
        /// </remarks>
        [HasMany]
        [InverseProperty(nameof(Column.CeMatrix))] // Bacause Cell with Column exists.
        public List<Column> Columns { get; set; } = new();

        /// <summary>
        ///     Ячейки матрицы ПСС
        /// </summary>
        /// <remarks>
        ///     Не хранятся в json, хранятся в csv.
        /// </remarks>
        [HasMany]
        public List<Cell> Cells { get; set; } = new();

        /// <summary>
        ///     Примечания матрицы ПСС
        /// </summary>
        /// <remarks>
        ///     Не хранятся в json, хранятся в csv.
        /// </remarks>
        [HasMany]        
        public List<CeMatrixComment> CeMatrixComments { get; set; } = new();

        public override ProjectVersionedEntityBase? TryGetParentProjectVersionedEntity() => null;
        public override bool HasParentProjectVersionedEntity() => false;
        public override Type GetParentProjectVersionedEntity_PropertyType() => throw new InvalidOperationException();
        public override int GetParentProjectVersionedEntity_Id() => throw new InvalidOperationException();
    }
}



//public override IEnumerable<ILastChangeEntity>? GetChildrenForIsDeleted()
//{
//    var result = new List<ILastChangeEntity>(); // CeMatrixParams.Count + CeMatrixDbFileReferences.Count + Causes.Count + Effects.Count + Intersections.Count + CeMatrixComments.Count
//    result.AddRange(CeMatrixParams);
//    result.AddRange(CeMatrixDbFileReferences);
//    result.AddRange(Causes);
//    foreach (var cause in Causes)
//        result.AddRange(cause.SubCauses);
//    result.AddRange(Effects);
//    result.AddRange(Intersections);
//    result.AddRange(CeMatrixComments);
//    return result;
//}