using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Стандартное свойство.
    /// </summary>
    [Resource]
    public class ParamInfo : Identifiable<int>
    {
        /// <summary>
        ///     Имя свойства
        /// </summary>
        [Attr]
        public string ParamName { get; set; } = @"";        

        /// <summary>
        ///     Тип объекта, содержащего данный параметр.
        /// </summary>
        [Attr]
        public string ObjectType { get; set; } = @"";

        /// <summary>
        ///     Описание свойства
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";
    }
}
