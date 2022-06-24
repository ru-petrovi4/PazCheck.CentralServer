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
        ///     Дата создания.
        /// </summary>
        [Attr]
        public UInt32 VersionNum { get; set; }

        /// <summary>        
        ///     Дата создания.
        /// </summary>
        [Attr]
        public DateTime TimeUtc { get; set; }

        /// <summary>
        ///     Создавший пользователь
        /// </summary>
        [Attr]
        public string User { get; set; } = @"";

        /// <summary>        
        ///     <para>Текстовое поле RW: Комментарий</para>
        /// </summary>
        [Attr]
        public string Comment { get; set; } = @"";        

        [HasMany]
        public List<DbFile> DbFiles { get; set; } = new();        

        [HasOne]
        public Project Project { get; set; } = null!;
    }
}
