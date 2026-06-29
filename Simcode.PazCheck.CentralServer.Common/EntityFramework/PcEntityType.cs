using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public abstract class PcEntityType : Identifiable<int>
    {
        /// <summary>  
        ///     Идентификатор типа
        /// </summary>        
        [Attr]
        public virtual string Type { get; set; } = @"";

        public string TypeLower { get; set; } = @"";

        /// <summary>
        ///     Название типа  
        /// </summary>        
        [Attr]
        public string Title { get; set; } = @"";

        /// <summary>
        ///     Описание        
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";

        /// <summary>
        ///     Иконка
        /// </summary>
        [HasOne]
        public DbFile? IconDbFile { get; set; }

        /// <summary>
        ///     Набор начальных стандартных свойств для данного типа
        /// </summary>
        [HasMany]
        public List<ParamInfo> StandardParamInfos { get; set; } = new();

        public override string ToString()
        {
            return Type;
        }
    }

    /// <summary>
    ///     Тип версии проекта
    /// </summary>
    [Resource]
    [Index(nameof(Type), IsUnique = true)]
    public class ProjectVersionType : PcEntityType
    {        
    }    

    /// <summary>
    ///     Тип события объекта модуля 'Мониторинг'
    /// </summary>
    [Resource]
    [Index(nameof(Type), IsUnique = true)]
    public class PcObjectEventType : PcEntityType
    {        
        /// <summary>
        ///     Шаблоны объектов модуля 'Мониторинг', связанные с данным типом
        /// </summary>
        [HasMany]
        public List<BasePcObject> BasePcObjects { get; set; } = new();
    }
}
