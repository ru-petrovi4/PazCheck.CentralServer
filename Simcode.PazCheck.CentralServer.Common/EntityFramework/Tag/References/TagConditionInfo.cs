using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Идентификатор состояния.
    /// </summary>
    [Resource]
    public class TagConditionInfo : Identifiable<int>
    {
        /// <summary>
        ///     <para>Текстовое поле RW: Идентификатор состояния</para>
        ///     <example>ALARM</example><example>PVHighHigh</example>
        /// </summary>
        [Attr]
        public string Identifier { get; set; } = @"";

        /// <summary>
        ///     <para>Текстовое поле RW: Тип тэга</para>
        ///     <example>Alarm</example>
        /// </summary>
        [Attr]
        public string TagType { get; set; } = @"";

        /// <summary>
        ///     <para>Текстовое поле RW: Описание идентификатора состояния</para>
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";
    }
}
