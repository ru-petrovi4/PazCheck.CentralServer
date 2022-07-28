using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Свойство
    /// </summary>    
    public abstract class Param : VersionEntityBase
    {
        /// <summary>
        ///     Имя свойства
        /// </summary>
        [Attr]
        public string ParamName { get; set; } = @"";

        /// <summary>
        ///     Значение свойства
        ///     <para>Текстовое поле RW: Значение свойства</para>
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     Единицы измерения
        ///     <para>Текстовое поле RW (выбор из списка (таблица EngineeringUnit) либо свое значение): Единицы измерения</para>
        /// </summary>
        [Attr]
        public string Eu { get; set; } = @"";

        /// <summary>
        ///     Ссылка на описание свойства
        /// </summary>
        [HasOne]
        public ParamInfo? ParamInfo { get; set; }
    }

    /// <summary>
    ///     Свойство тэга
    /// </summary>
    [Resource]
    public class TagParam : Param
    {
        /// <summary>
        ///     Родительский тэг
        /// </summary>
        [HasOne]
        public Tag Tag { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => Tag;
    }

    /// <summary>
    ///     Свойство исполнительного механизма
    /// </summary>
    [Resource]
    public class ActuatorParam : Param
    {
        /// <summary>
        ///     Родительский тэг
        /// </summary>
        [HasOne]
        public Tag Tag { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => Tag;
    }

    /// <summary>
    ///     Свойство модели исполнительного механизма
    /// </summary>
    [Resource]
    public class BaseActuatorParam : Param
    {
        /// <summary>
        ///     Родительская модель исполнительного механизма
        /// </summary>
        [HasOne]
        public BaseActuator BaseActuator { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => BaseActuator;
    }

    /// <summary>
    ///     Свойство матрицы ПСС
    /// </summary>
    [Resource]
    public class CeMatrixParam : Param
    {
        /// <summary>
        ///     Родительская матрица
        /// </summary>
        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => CeMatrix;
    }
}