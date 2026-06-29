using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
	/// <summary>
    ///     Событие объекта модуля 'Мониторинг'    
    /// </summary>
    /// <remarks>
    ///     Событие (event) (ISO 19440:2007)
    ///     Конструкция, отображающая ожидаемый или неожиданный факт, указывающий на изменение состояния предприятия или его окружения.
    ///     Модуль 'Мониторинг'
    /// </remarks>
    [Resource]    
    public class PcObjectEvent : Identifiable<int>, ICreateDeleteInfoEntity
    {
        /// <summary>
        ///     Начальное время события
        /// </summary>
        [Attr]
        public DateTime BeginTimeUtc { get; set; }

        /// <summary>
        ///     Конечное время события
        /// </summary>
        [Attr]
        public DateTime? EndTimeUtc { get; set; }        

        /// <summary>
        ///     Свойства события
        /// </summary>
        /// <remarks>
        ///     Могут переопределять свойства типа события.
        ///     (Url encoded name-values collection).        
        /// </remarks> 
        public string Params { get; set; } = @"";        

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

        [HasMany]
        public List<PcObjectEventDbFileReference> PcObjectEventDbFileReferences { get; set; } = new();

        /// <summary>
        ///     PcObjectEvent.Type
        /// </summary>        
        [Attr]
        public string PcObjectEventType { get; set; } = null!;

        public string PcObjectEventTypeLower { get; set; } = @"";

        /// <summary>
        ///     Родительский объект
        /// </summary>        
        [HasOne]
        [ForeignKey(nameof(PcObjectId))]
        public PcObject PcObject { get; set; } = null!;

        /// <summary>
        ///     Родительский объект
        /// </summary>        
        public int PcObjectId { get; set; }

        // <summary>
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

        ///// <summary>
        /////     Obsolete
        ///// </summary> 
        //[Attr]
        //[NotMapped]
        //public DateTime? _DeleteTimeUtc { get; set; }

        ///// <summary>
        /////     Obsolete
        ///// </summary> 
        //[Attr]
        //[NotMapped]
        //public string _DeleteUser { get; set; } = @"";

        /// <summary>
        ///     Удалена ли сущность
        /// </summary>
        [Attr]
        public bool _IsDeleted { get; set; }

        public override string ToString()
        {
            return PcObjectEventType;
        }
    }
}
