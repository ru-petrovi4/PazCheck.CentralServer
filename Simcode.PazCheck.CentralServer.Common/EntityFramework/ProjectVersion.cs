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
    ///     Версия проекта
    /// </summary>
    [Resource]
    [Index(nameof(ProjectId), nameof(VersionNum), IsUnique = true)]
    public class ProjectVersion : Identifiable<int>
    {
        /// <summary>        
        ///     Номер версии
        /// </summary>
        [Attr]
        public UInt32 VersionNum { get; set; }

        /// <summary>        
        ///     Время создания версии
        /// </summary>
        [Attr]
        public DateTime TimeUtc { get; set; }

        /// <summary>
        ///     Создавший пользователь
        /// </summary>
        [Attr]
        public string User { get; set; } = @"";

        /// <summary>        
        ///     Время, когда были утверждены изменения в версии
        /// </summary>
        [Attr]
        public DateTime? Active_TimeUtc { get; set; }

        /// <summary>
        ///     Пользователь, утвердивший изменения в версии
        /// </summary>
        [Attr]
        public string? Active_SupervisorUser { get; set; } = @"";        

        /// <summary>
        ///     Комментарий к версии        
        /// </summary>
        [Attr]
        public string Comment { get; set; } = @"";

        /// <summary>
        ///     Статус версии             
        /// </summary>
        /// <remarks>
        ///     0 - обычная.
        ///     1 - запрошена на активную.
        ///     2 - активная.   
        /// </remarks>
        [Attr]
        public int Status { get; set; }

        /// <summary>
        ///     Тип версии проекта
        /// </summary>        
        [Attr]
        public string ProjectVersionType { get; set; } = @"";

        /// <summary>
        ///     Свойства версии
        /// </summary>
        /// <remarks>
        ///     Пары ("имя", "значение") (Url Encoded).
        /// </remarks>
        public string Params { get; set; } = @"";

        /// <summary>
        ///     Свойства версии
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
        public List<ProjectVersionDbFileReference> ProjectVersionDbFileReferences { get; set; } = new();

        /// <summary>
        ///     Родительский проект
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; } = null!;

        /// <summary>
        ///     Родительский проект
        /// </summary>
        public int ProjectId { get; set; }
    }
}
