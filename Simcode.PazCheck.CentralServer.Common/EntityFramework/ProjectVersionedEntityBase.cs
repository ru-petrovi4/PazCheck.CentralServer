using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public abstract class ProjectVersionedEntityBase : VersionedEntityBase, IStringIdentifierEntity
    {
        /// <summary>
        ///     Идентификатор, уникальный внутри версии проекта
        /// </summary>
        [Attr]
        public virtual string Identifier { get; set; } = @"";

        /// <summary>
        ///     Вычислимое поле
        /// </summary>
        public virtual string IdentifierLower { get; set; } = @"";

        /// <summary>
        ///     Пользовтаель, который эксклюзивно заблокировал (забронировал) эту сущность для редактирования
        /// </summary>
        [Attr]
        public string _LockedByUser { get; set; } = @"";

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

        public override string ToString()
        {
            return Identifier;
        }
    }
}
