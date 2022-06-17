using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Свойство тэга или исполнительного механизма.
    /// </summary>
    public class Param : VersionEntity
    {
        /// <summary>
        ///     <para>Текстовое поле RW: Имя свойства</para>
        /// </summary>
        [Attr]
        public string ParamName { get; set; } = @"";

        /// <summary>
        ///     <para>Текстовое поле RW: Значение свойства</para>
        /// </summary>
        [Attr]
        public string Value { get; set; } = @"";

        /// <summary>
        ///     <para>Текстовое поле RW (выбор из списка либо свое значение): Единицы измерения</para>
        /// </summary>
        [Attr]
        public string Eu { get; set; } = @"";

        /// <summary>
        ///     Reserved
        /// </summary>
        [Attr]
        public string Type { get; set; } = @"";

        /// <summary>
        ///     <para>Текстовое поле RW: Описание свойства</para>
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";
    }
}
