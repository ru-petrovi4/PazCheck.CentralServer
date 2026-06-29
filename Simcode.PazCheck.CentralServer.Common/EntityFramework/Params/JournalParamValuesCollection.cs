using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Ssz.Utils;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Свойство с накопленными значениями
    /// </summary>
    [Resource]    
    public class JournalParamValuesCollection : Identifiable<int>
    {
        /// <summary>
        ///     Имя свойства
        /// </summary>        
        [Attr]
        public string ParamName { get; set; } = @"";

        public string ParamNameLower { get; set; } = @"";

        /// <summary>
        ///     Поля с мета-данными параметра
        /// </summary>
        /// <remarks>
        ///     (Url encoded name-values collection).
        /// </remarks>        
        public string MetadataFields { get; set; } = @"";

        /// <summary>
        ///     Поля с мета-данными параметра
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
        ///     Время текущего значения
        ///     The number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.
        /// </summary>  
        [Attr]
        public long CurrentValue_TimestampUtc { get; set; }

        /// <summary>
        ///     Текущее значение 
        /// </summary>
        [Attr]
        public float? CurrentValue { get; set; }        

        /// <summary>
        ///     Вещественные исторические значения
        /// </summary>    
        [HasMany]
        public List<FloatJournalParamValue> FloatValues { get; set; } = new();        

        public int PcObjectId { get; set; }

        /// <summary>
        ///     Ссылка на родительский объект модуля 'Мониторинг'
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(PcObjectId))]
        public PcObject PcObject { get; set; } = null!;        
    }
}
