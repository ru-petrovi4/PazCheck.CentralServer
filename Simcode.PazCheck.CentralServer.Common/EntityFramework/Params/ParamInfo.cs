using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Hosting;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Описание свойства объекта    
    /// </summary>    
    [Resource]
    public class ParamInfo : Identifiable<int>
    {
        /// <summary>
        ///     Имя свойства
        /// </summary>
        [Attr]
        public string ParamName { get; set; } = @"";

        public string ParamNameLower { get; set; } = @"";

        /// <summary>
        ///     Значение свойства по умолчанию   
        /// </summary>
        [Attr]
        public string DefaultValue { get; set; } = @"";

        /// <summary>
        ///     Поля с мета-данными свойства.        
        /// </summary>
        /// <remarks>
        ///     (Url encoded name-values collection)        
        /// </remarks>        
        public string MetadataFields { get; set; } = @"";

        /// <summary>
        ///     Поля с мета-данными свойства.        
        /// </summary>
        [Attr]
        [NotMapped]
        public CaseInsensitiveOrderedDictionary<string?> MetadataFieldsDictionary
        {
            get
            {
                return NameValueCollectionHelper.Parse(MetadataFields);
            }
            set
            {
                MetadataFields = NameValueCollectionHelper.GetNameValueCollectionString(value);
            }
        }

        /// <summary>
        ///     Связанные типы версий проекта
        /// </summary>
        [HasMany]
        public List<ProjectVersionType> ProjectVersionTypes { get; set; } = new();        

        /// <summary>
        ///     Связанные типы событий объектов модуля 'Мониторинг'
        /// </summary>
        [HasMany]
        public List<PcObjectEventType> PcObjectEventTypes { get; set; } = new();
    }
}


///// <summary>
/////     Ссылка на описание свойства, если есть
///// </summary>
//[HasOne]
//[ForeignKey(nameof(ParamName))]
//public ParamDesc? ParamDesc { get; set; }
