using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{    
    public abstract class JournalParam : Identifiable<int>
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
        [Attr]
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
    }

    /// <summary>
    ///     Описание свойства, для которого накапливается история значений
    /// </summary>
    [Resource]
    public class BasePcObjectJournalParam : JournalParam
    {
        public int BasePcObjectId { get; set; }

        /// <summary>
        ///     Ссылка на родительский объект модуля 'Мониторинг'
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(BasePcObjectId))]
        public BasePcObject BasePcObject { get; set; } = null!;
    }

    /// <summary>
    ///     Описание свойства, для которого накапливается история значений
    /// </summary>
    [Resource]
    public class PcObjectJournalParam : JournalParam
    {
        public int PcObjectId { get; set; }

        /// <summary>
        ///     Ссылка на родительский объект модуля 'Мониторинг'
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(PcObjectId))]
        public PcObject PcObject { get; set; } = null!;
    }
}
