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
    ///     Класс исполнительных механизмов.
    /// </summary>
    public class BaseActuator : Identifiable<int>
    {
        /// <summary>        
        ///     <para>Текстовое поле RW: Название класса исполнительных механизмов</para>
        /// </summary>
        [Attr]
        public string Title { get; set; } = @"";

        /// <summary>        
        ///     <para>Текстовое поле RW: Описание</para>
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";

        [HasMany]
        public List<Param> Params { get; set; } = new List<Param>();

        [HasOne]
        public Project Project { get; set; } = null!;
    }
}
