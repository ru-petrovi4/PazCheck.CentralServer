using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Стандартное свойство.
    /// </summary>
    [Resource]
    public class StandardParam : Identifiable<int>
    {        
        /// <summary>
        ///     <para>Текстовое поле RW: Имя свойства</para>
        /// </summary>
        [Attr]
        public string ParamName { get; set; } = @"";        

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
