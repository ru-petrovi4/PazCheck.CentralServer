using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Единицы измерения.
    /// </summary>
    [Resource]
    public class EngineeringUnit : Identifiable<int>
    {
        /// <summary>
        ///     Обозначение единиц измерения
        ///     <para>Текстовое поле RW: Обозначение</para>
        ///     <example>с</example><example>м/с</example>
        /// </summary>
        [Attr]
        public string Eu { get; set; } = @"";

        /// <summary>
        ///     Описание единиц измерения
        ///     <para>Текстовое поле RW: Описание</para>
        ///     <example>Секунды</example><example>Метры в секунду</example>
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";
    }
}
