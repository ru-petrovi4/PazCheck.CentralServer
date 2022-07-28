using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.Collections.Generic;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Категория устройства.
    /// </summary>
    [Resource]
    public class BaseActuatorType : Identifiable<int>
    {
        /// <summary>
        ///     Категория оборудования (клапан, насос и т.д.)
        ///     <para>Текстовое поле RW: Категория устройства</para>
        ///     <para>Tooltip: Категория оборудования</para>        /
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
        public List<ParamInfo> StandardParamInfos { get; set; } = new();
    }
}
