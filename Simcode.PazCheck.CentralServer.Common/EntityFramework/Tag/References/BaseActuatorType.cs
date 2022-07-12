using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.Collections.Generic;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Тип устройства.
    /// </summary>
    [Resource]
    public class BaseActuatorType : Identifiable<int>
    {
        /// <summary>
        ///     <para>Текстовое поле RW: Тип устройства</para>
        ///     <para>Tooltip: Вид оборудования</para>
        ///     <example>Клапан</example><example>Насос</example>
        /// </summary>
        [Attr]
        public string Type { get; set; } = @"";

        /// <summary>
        ///     Reserved        
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";

        /// <summary>
        ///     Набор начальных стандартных параметров для данной категории.
        /// </summary>
        [HasMany]
        public List<StandardParam> StandardParams { get; set; } = new();
    }
}
