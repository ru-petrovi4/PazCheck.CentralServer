using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Effect : VersionEntity
    {
        /// <summary>
        ///     Порядковый номер внутри диаграммы
        /// </summary>
        [Attr]
        public int Num { get; set; }

        [Attr]
        public string TagName { get; set; } = @"";        

        [Attr]
        public string TagConditionString { get; set; } = @"";

        [Attr]
        public string CustomFieldValues { get; set; } = @"";

        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;
    }
}