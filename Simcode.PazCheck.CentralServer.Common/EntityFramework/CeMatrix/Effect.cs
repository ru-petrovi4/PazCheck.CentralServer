using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Effect : VersionEntity
    {
        /// <summary>
        ///     Порядковый номер внутри диаграммы
        /// </summary>
        [Attr(PublicName = "num")]
        public int Num { get; set; }

        [Attr(PublicName="name")]
        public string TagName { get; set; } = @"";        

        [Attr(PublicName = "state")]
        public string TagConditionString { get; set; } = @"";

        [Attr(PublicName = "descr")]
        public string CustomFieldValues { get; set; } = @"";        
    }
}
