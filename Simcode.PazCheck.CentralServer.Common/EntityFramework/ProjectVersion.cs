using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
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
        ///     Комментарий к версии
        ///     <para>Текстовое поле RW: Комментарий</para>
        /// </summary>
        [Attr]
        public string Comment { get; set; } = @"";        

        [HasMany]
        public List<DbFile> DbFiles { get; set; } = new();        

        /// <summary>
        ///     Родительский проект
        /// </summary>
        [HasOne]
        public Project Project { get; set; } = null!;
    }
}
