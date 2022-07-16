using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Свойство тэга или исполнительного механизма.
    /// </summary>    
    public abstract class Param : VersionEntityBase
    {
        /// <summary>
        ///     Имя свойства
        /// </summary>
        [Attr]
        public string ParamName { get; set; } = @"";

        /// <summary>
        ///     <para>Текстовое поле RW: Значение свойства</para>
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     <para>Текстовое поле RW (выбор из списка (таблица EngineeringUnit) либо свое значение): Единицы измерения</para>
        /// </summary>
        [Attr]
        public string Eu { get; set; } = @"";

        [HasOne]
        public ParamInfo? ParamInfo { get; set; }
    }

    [Resource]
    public class TagParam : Param
    {
        [HasOne]
        public Tag Tag { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => Tag;
    }

    [Resource]
    public class ActuatorParam : Param
    {
        [HasOne]
        public Tag Tag { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => Tag;
    }

    [Resource]
    public class BaseActuatorParam : Param
    {
        [HasOne]
        public BaseActuator BaseActuator { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => BaseActuator;
    }

    [Resource]
    public class CeMatrixParam : Param
    {
        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => CeMatrix;
    }
}