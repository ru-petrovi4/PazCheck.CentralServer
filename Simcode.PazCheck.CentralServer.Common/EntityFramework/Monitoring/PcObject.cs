// Copyright (c) 2021
// All rights reserved by Simcode

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Ssz.Utils;
using Ssz.Utils.DataAccess;
using Microsoft.EntityFrameworkCore;
using Simcode.PazCheck.CentralServer.Common.Helpers;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Объект модуля 'Мониторинг'
    /// </summary>
    /// <remarks>
    ///     Объект предприятия (enterprise object) [ИСО 19439:2006].
    ///     Конструкция, отображающая единицу информации предприятия и описывающая реальную сущность, которую можно осмыслить как целое.
    /// </remarks>
    [Resource]    
    public class PcObject : Identifiable<int>, ICreateDeleteInfoEntity
    {
        /// <summary>     
        ///     Идентификатор        
        /// </summary>
        [Attr]
        public string Identifier { get; set; } = @"";

        public string IdentifierLower { get; set; } = @"";

        /// <summary>
        ///     Родительская установка
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; } = null!;

        /// <summary>
        ///     Родительская установка
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        ///     Информация о виджетах пользователя
        /// </summary>
        [Attr]
        public string Widgets { get; set; } = @"";

        /// <summary>
        ///     Ссылка на шаблон объекта. 
        ///     <para>Заполнятется автоматически из свойства Params['PcObject:Template']</para>
        /// </summary>
        [HasOne]        
        public BasePcObject BasePcObject { get; set; } = null!;

        /// <summary>
        ///     Дочерние объекты
        /// </summary>
        [HasMany]
        public List<PcObject> Children { get; set; } = new();

        /// <summary>
        ///     Родительский объект
        ///     <para>Заполнятется автоматически из свойства Params['PcObject:Parent']</para>
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(ParentId))]
        public PcObject? Parent { get; set; }        

        [NotMapped]
        public string? Temp_NewParent_Identifier { get; set; }

        [NotMapped]
        public string? Temp_NewTemplate_Identifier { get; set; }             

        /// <summary>
        ///     Родительский объект
        /// </summary>        
        public int? ParentId { get; set; }

        /// <summary>
        ///     Уровень в дереве (0..)
        /// </summary>
        /// <remarks>
        ///     Вычисляется при изменении дерева на основе ParentId
        /// </remarks>
        [Attr]
        public int Level { get; set; } = 0;

        /// <summary>
        ///     Текущий цвет индекса безопасности. Если индекс неизвестен: -100. Зеленый: 100; Желтый: 50: Красный: 0;
        ///     Не соответствует реальному индексу безопасности! Соответствует только цвет!
        ///     <para>Заполнятется автоматически на основе свойства Params['Data:*SafetyIndex*']</para>
        /// </summary>
        [Attr]
        public double SafetyIndex { get; set; } = 0;

        /// <summary>
        ///     Новое значение индекса безопасности.
        ///     Соответствует реальному индексу безопасности!
        /// </summary>
        [NotMapped]
        public double Temp_NewSafetyIndex { get; set; }

        /// <summary>
        ///     Описание текущего индекса безопасности.
        ///     <para>Заполнятется автоматически из свойства Params['Data:SafetyIndexDesc']</para>
        /// </summary>
        [Attr]
        public string SafetyIndexDesc { get; set; } = @"";

        /// <summary>
        ///     Время последнего изменения значения SafetyIndex.
        /// </summary>
        [Attr]
        public DateTime SafetyIndex_LastChangeTimeUtc { get; set; }

        /// <summary>
        ///     Весовой коэффициент в тепловой карте
        ///     <para>Заполнятется автоматически из свойства Params['SafetyIndexK'] * Params['SafetyIndexK2']</para>
        /// </summary>
        [Attr]        
        public double K { get; set; }        

        /// <summary>
        ///     Набор свойств, для которых накапливается история значений
        /// </summary>
        [HasMany]
        public List<PcObjectJournalParam> JournalParams { get; set; } = new();

        /// <summary>
        ///     Набор накопленных значений свойств.
        ///     Создается даже при TrendEnabled = false
        /// </summary>
        [HasMany]
        public List<JournalParamValuesCollection> JournalParamValuesCollections { get; set; } = new();

        /// <summary>
        ///     Список событий объекта
        /// </summary>
        [HasMany]
        public List<PcObjectEvent> PcObjectEvents { get; set; } = new();

        /// <summary>
        ///     Свойства объекта
        /// </summary>
        /// <remarks>
        ///     Могут переопределять свойства шаблона объекта.
        ///     (Url encoded name-values collection).        
        /// </remarks>        
        public string Params { get; set; } = @"";

        /// <summary>
        ///     Свойства объекта
        /// </summary>
        [Attr]
        [NotMapped]
        public CaseInsensitiveOrderedDictionary<string?> ParamsDictionary
        {
            get
            {
                return NameValueCollectionHelper.Parse(Params);
            }
            set
            {
                Params = NameValueCollectionHelper.GetNameValueCollectionString(value);
            }
        }

        /// <summary>
        ///     Ссылки на приложенные файлы
        /// </summary>
        [HasMany]
        public List<PcObjectDbFileReference> PcObjectDbFileReferences { get; set; } = new();

        /// <summary>
        ///     Время создания сущности
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу.
        /// </remarks>
        [Attr]
        public DateTime _CreateTimeUtc { get; set; }

        /// <summary>
        ///     Пользователь, создавший сущность
        /// </summary>        
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу, если есть HttpContext или задан dbContext.User.
        /// </remarks> 
        [Attr]
        public string _CreateUser { get; set; } = @"";

        /// <summary>
        ///     Время последнего изменения
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу.
        /// </remarks>
        [Attr]
        public DateTime _LastChangeTimeUtc { get; set; }

        /// <summary>
        ///     Последний пользователь, изменивший сущность
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу, если есть HttpContext или задан dbContext.User.
        /// </remarks> 
        [Attr]
        public string _LastChangeUser { get; set; } = @"";        

        /// <summary>
        ///     Удалена ли сущность
        /// </summary>
        [Attr]
        public bool _IsDeleted { get; set; }        

        public override string ToString()
        {
            return Identifier;
        }        

        public string ComputeValueOfSszQueries(string? originalString,
            IReadOnlyDictionary<string, string?> pcObjectParamsDictionary, // For optimization
            IReadOnlyDictionary<string, string?>? basePcObjectParamsDictionary, // For optimization
            CsvDb? csvDb = null,
            IterationInfo? iterationInfo = null)
        {
            if (string.IsNullOrEmpty(originalString))
                return @"";

            return SszQueryHelper.ComputeValueOfSszQueries(
                originalString,
                (c, iterationInfo) =>
                {
                    if (String.Equals(c, @"%(TAG)", StringComparison.InvariantCultureIgnoreCase))
                        return Identifier;
                    if (String.Equals(c, @"%(TAGNUM)", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int index = Identifier.IndexOfAny(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']);
                        if (index > 0)
                            return Identifier.Substring(index);
                        else
                            return c;
                    }
                    if (c.Length < 3)
                        return c;
                    var paramName = c.Substring(2, c.Length - 3).Trim(); // Remove '%(' and ')'
                    var paramValue = PazCheckDbHelper.GetParamValue(
                        pcObjectParamsDictionary,
                        basePcObjectParamsDictionary,
                        paramName);
                    return ComputeValueOfSszQueries(
                        paramValue,
                        pcObjectParamsDictionary,
                        basePcObjectParamsDictionary,
                        csvDb,
                        iterationInfo);
                },
                csvDb,
                iterationInfo);
        }
    }
}
