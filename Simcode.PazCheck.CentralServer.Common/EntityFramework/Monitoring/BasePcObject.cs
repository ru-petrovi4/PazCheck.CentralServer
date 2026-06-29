using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Шаблон объекта модуля 'Мониторинг'
    /// </summary>
    /// <remarks>
    ///     Обобщенный или абстрактный объект предприятия (enterprise object) [ИСО 19439:2006].
    ///     Конструкция, отображающая единицу информации предприятия и описывающая обобщенную или абстрактную сущность, которую можно осмыслить как целое.    
    /// </remarks>
    [Resource]    
    public class BasePcObject : Identifiable<int>, ICreateDeleteInfoEntity
    {
        /// <summary>     
        ///     Идентификатор шаблона       
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
        ///     Набор описаний свойств, для которых накапливается история значений
        /// </summary>
        [HasMany]
        public List<BasePcObjectJournalParam> JournalParams { get; set; } = new();

        /// <summary>
        ///     Типы событий объектов данного шаблона
        /// </summary>
        [HasMany]
        public List<PcObjectEventType> PcObjectEventTypes { get; set; } = new();

        /// <summary>
        ///     Свойства шаблона объекта
        /// </summary>
        /// <remarks>
        ///     (Url encoded name-values collection).        
        /// </remarks>
        public string Params { get; set; } = @"";

        /// <summary>
        ///     Свойства шаблона объекта
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
        public List<BasePcObjectDbFileReference> BasePcObjectDbFileReferences { get; set; } = new();        

        /// <summary>
        ///     Список ссылок на объекты данного шаблона
        /// </summary>
        [HasMany]
        public List<PcObject> PcObjects { get; set; } = new();

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
            return Identifier;
        }
    }
}
