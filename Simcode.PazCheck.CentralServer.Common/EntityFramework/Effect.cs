using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Effect : Identifiable<int>
    {
        [Attr(PublicName = "num")]
        public int Num { get; set; } // Порядковый номер внутри диаграммы

        [Attr(PublicName="name")]
        public string Name { get; set; } = @"";

        [Attr(PublicName = "descr")]
        public string Descr { get; set; } = @""; //Некая описательная часть

        [Attr(PublicName = "state")]
        public string State { get; set; } = @"";

        public int ProjectId { get; set; }

        [HasOne(PublicName = "project")]
        public Project Project { get; set; } = null!;

        [HasOne(PublicName="identity")]
        public Identity Identity { get; set; } = null!;

        //For serialization and deserialization
        [NotMapped]
        public int IdentityKey { get; set; }

        [NotMapped]
        public int Key { get; set; }
    }
}
