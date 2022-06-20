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
        ///     Имя свойства
        /// </summary>
        [Attr]
        public string ParamName { get; set; } = @"";        

        /// <summary>
        ///     Reserved
        /// </summary>
        [Attr]
        public string Type { get; set; } = @"";

        /// <summary>
        ///     Описание свойства
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";
    }
}
