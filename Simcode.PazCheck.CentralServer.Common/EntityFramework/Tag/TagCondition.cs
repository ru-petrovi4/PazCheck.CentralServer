
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Microsoft.EntityFrameworkCore;
using Ssz.Utils;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Событие тэга
    /// </summary>
    /// <remarks>
    ///     ConditionCategory, AeCondition, DaCondition - Unique for the Tag    
    /// </remarks>
    [Resource]    
    public class TagCondition : VersionedEntityBase
    {
        /// <summary>
        ///     Категория события
        /// </summary>
        /// <remarks>
        ///     Категория события: команда на закрытие, команда на открытие, факт открытия, факт закрытия.        
        ///     Возможно, с '!' в начале.
        ///     <see cref="PazCheckConstants"/>
        /// </remarks>
        [Attr]
        public string ConditionCategory { get; set; } = @"";

        /// <summary>
        ///     Строка события для поиска в журнале событий
        /// </summary>
        /// <remarks>
        ///     Tag|Condition=&amp;Action=&amp;Priority=&amp;Value=&amp;Description=       
        /// </remarks>
        [Attr]
        public string AeCondition { get; set; } = @"";

        /// <summary>
        ///     Строка события для получения из БДРВ
        /// </summary>
        /// <remarks>
        ///     Tag.Property=Value.        
        /// </remarks>
        [Attr]
        public string DaCondition { get; set; } = @"";

        /// <summary>
        ///     Краткое имя состояния для отображения пользователю в матрицах ПСС
        /// </summary>
        /// <remarks>
        ///     null, если не определено. Пустая строка, если нужно отображать как пустой символ.
        ///     Может быть несколько через запятую.
        /// </remarks>
        [Attr]
        public string? SymbolToDisplay { get; set; }

        /// <summary>
        ///     Может ли быть причиной в матрицах ПСС        
        /// </summary>
        [Attr]
        public bool CanBeCause { get; set; } = true;

        /// <summary>
        ///     Может ли быть следствием в матрицах ПСС        
        /// </summary>
        [Attr]
        public bool CanBeEffect { get; set; } = true;        

        public int TagId { get; set; }

        /// <summary>
        ///     Родительский тэг
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(TagId))]
        public Tag Tag { get; set; } = null!;

        public override ProjectVersionedEntityBase? TryGetParentProjectVersionedEntity() => Tag;
        public override bool HasParentProjectVersionedEntity() => true;
        public override Type GetParentProjectVersionedEntity_PropertyType() => typeof(Tag);
        public override int GetParentProjectVersionedEntity_Id() => TagId;

        public override string ToString()
        {
            return $"({SymbolToDisplay})" + CsvHelper.FormatForCsv(@":",
                [ ConditionCategory,
                AeCondition,
                DaCondition,
                new Any(CanBeCause).ValueAsString(false),
                new Any(CanBeEffect).ValueAsString(false) ]);
        }
    }
}