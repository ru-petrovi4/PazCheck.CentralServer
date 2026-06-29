using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Ssz.Utils;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{    
    public abstract class RowOrColumnBase : VersionedEntityBase
    {
        /// <summary>
        ///     Строка, определяющая порядок строк или столбцов внутри матрицы        
        /// </summary>
        /// <remarks>
        ///     HEX представление числа, с четным количеством знаков. Примеры: '01', 'a2', 'a201'
        /// </remarks>
        [Attr]
        public string Order { get; set; } = @"";

        /// <summary>
        ///     Заголовок вспомогательной (текстовой) строки или столбца либо имя тэга.
        /// </summary>        
        [Attr]
        public string Header { get; set; } = @"";

        /// <summary>
        ///     Ссылка на состояние тэга
        /// </summary>
        /// <remarks>
        ///     null, если вспомогательная (текстовая) строка или столбец, т.е. заголовок.
        /// </remarks>
        [Attr]
        public string? TagCondition_SymbolToDisplay { get; set; }           

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

        public bool IsMainArea()
        {
            return !Order.StartsWith("00");
        }
    }    

    /// <summary>
    ///     Строка в матрице ПСС
    /// </summary>
    [Resource]    
    public class Row : RowOrColumnBase
    {
        [HasMany]        
        public List<Cell> Cells { get; set; } = new();        
    }
        
    /// <summary>
    ///     Столбец в матрице ПСС
    /// </summary>
    [Resource]    
    public class Column : RowOrColumnBase
    {
        [HasMany]
        public List<Cell> Cells { get; set; } = new();
    }

    public static class RowOrColumnHelper
    {
        #region public functions       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowOrColumn"></param>
        /// <returns></returns>
        public static string GetDesc(RowOrColumnBase rowOrColumn)
        {   
            if (rowOrColumn.TagCondition_SymbolToDisplay is null) // Custom Field
                return GetDesc(null, null, rowOrColumn.Header);
            else
                return GetDesc(rowOrColumn.Header, rowOrColumn.TagCondition_SymbolToDisplay, null);
        }

        public static string GetDesc(string? tagName, string? tagCondition_SymbolToDisplay, string? customField)
        {
            if (customField is null)
                return "TAG(s): " + (String.IsNullOrEmpty(tagName) ? "<Empty>" : tagName) +
                            (String.IsNullOrEmpty(tagCondition_SymbolToDisplay) ? "; Condition: <Empty>" : "; Condition: " + tagCondition_SymbolToDisplay);
            else
                return "Header: " + (String.IsNullOrEmpty(customField) ? "<Empty>" : customField);
        }

        #endregion
    }
}
