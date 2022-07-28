using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Описание идентификатора состояния тэга.
    /// </summary>
    [Resource]
    public class TagConditionInfo : Identifiable<int>
    {
        /// <summary>
        ///     Идентификатор состояния (ALARM, PVHighHigh и т.д.)
        ///     <para>Текстовое поле RW: Идентификатор состояния</para>
        ///     <example>ALARM</example><example>PVHighHigh</example>
        /// </summary>
        [Attr]
        public string Identifier { get; set; } = @"";

        /// <summary>
        ///     Тип родительского тэга
        ///     <para>Текстовое поле RW: Тип состояния</para>
        ///     <example>Alarm</example>
        /// </summary>
        [Attr]
        public string TagType { get; set; } = @"";

        /// <summary>
        ///     Описание идентификатора состояния
        ///     <para>Текстовое поле RW: Описание идентификатора состояния</para>
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";
    }
}
