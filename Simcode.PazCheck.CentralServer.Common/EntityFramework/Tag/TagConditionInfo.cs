using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Тип состояния тэга
    /// </summary>
    [Resource]
    public class TagConditionInfo : Identifiable<int>
    {
        /// <summary>
        ///     Строка состояния для поиска в журнале событий
        /// </summary>
        /// <remarks>
        ///     Tag.ConditionType|TripValue|Desc.
        ///     Не может быть пустым. Уникально для каждого состояния данного тэга.
        /// </remarks>
        [Attr]
        public string AeCondition { get; set; } = @"";

        /// <summary>
        ///     Строка состояния для получения из БДРВ
        /// </summary>
        /// <remarks>
        ///     Tag.Property=Value.
        ///     Может быть пустым.
        /// </remarks>
        [Attr]
        public string DaCondition { get; set; } = @"";

        /// <summary>
        ///     Краткое имя состояния для отображения пользователю в матрицах ПСС
        /// </summary>
        /// <remarks>
        ///     null, если еще не определено. Пустая строка, если нужно отображать как пустой символ.
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
    }
}
