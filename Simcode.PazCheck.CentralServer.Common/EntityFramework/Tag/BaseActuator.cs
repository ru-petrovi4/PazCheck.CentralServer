using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Модель исполнительного механизма
    /// </summary>
    [Resource]
    public class BaseActuator : VersionEntityBase
    {
        /// <summary>   
        ///     Наименование модели оборудования. Поле не версионируется.
        ///     <para>Текстовое поле RW: Наименование модели</para>
        ///     <para>Tooltip: Наименование модели оборудования</para>
        /// </summary>
        [Attr]
        public string Title { get; set; } = @"";

        /// <summary>     
        ///     Номер модели оборудования в каталоге производителя. Поле не версионируется.
        ///     <para>Текстовое поле RW: Код модели</para>
        ///     <para>Tooltip: Номер модели оборудования в каталоге производителя</para>
        /// </summary>
        [Attr]
        public string Code { get; set; } = @"";

        /// <summary>   
        ///     Производитель оборудования. Поле не версионируется.
        ///     <para>Текстовое поле RW: Производитель</para>
        ///     <para>Tooltip: Производитель оборудования</para>
        /// </summary>
        [Attr]
        public string Manufacturer { get; set; } = @"";

        /// <summary>   
        ///     Комментарий, дополнительная информация о модели. Поле не версионируется.
        ///     <para>Текстовое поле RW: Примечание</para>
        ///     <para>Tooltip: Комментарий, дополнительная информация о модели</para>
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";

        [Attr]
        public string _LockedByUser { get; set; } = @"";

        /// <summary>
        ///     Категория оборудования (клапан, насос и т.д.)
        ///     <para>Текстовое поле RW (выбор из списка (таблица BaseActuatorType) либо свое значение): Категория</para>
        ///     <para>Tooltip: Категория оборудования</para>
        /// </summary>        
        [Attr]
        public BaseActuatorType BaseActuatorType { get; set; } = null!;

        [HasMany]
        public List<BaseActuatorParam> BaseActuatorParams { get; set; } = new();

        [HasMany]
        public List<BaseActuatorDbFileReference> BaseActuatorDbFileReferences { get; set; } = new();

        /// <summary>
        ///     Родительский проект
        /// </summary>
        [HasOne]
        public Project Project { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => Project;
    }
}
