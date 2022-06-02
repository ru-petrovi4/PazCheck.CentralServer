using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Cause : Identifiable<int>
    {
        [Attr(PublicName = "num")]
        public int Num { get; set; } // Порядковый номер внутри диаграммы

        [Attr(PublicName="name")]
        public string Name { get; set; } = @"";

        [Attr(PublicName="limit")]
        public int Limit { get; set; }

        [Attr(PublicName="delay")]
        public int Delay { get; set; }

        [Attr(PublicName="complex")]
        public bool Complex { get; set; }

        public int ProjectId { get; set; }

        [HasOne(PublicName="project")]
        public Project Project { get; set; }

        [HasMany(PublicName="identities")]
        public List<Identity> Identities { get; set; }

        [HasMany(PublicName = "subcauses")]
        public List<SubCause> SubCauses { get; set; }

        [NotMapped]
        public ICollection<int> IdentitiesKeys { get; set; }

        [NotMapped]
        public int Key { get; set; }

        public Cause()
        {
            Limit = 0;
            Delay = 0;
            Complex = false;
            Identities = new List<Identity>();
            SubCauses = new List<SubCause>();
            IdentitiesKeys = new List<int>();
            Key = 0;
        
        }
        public Identity? ProccessEvent(LogRecord logRecord)
        {
            return Identities.FirstOrDefault(identity => identity.ProccessEvent(logRecord));
        }

        public void MapIdetitiesKeys()
        {
            foreach (var identity in Identities)
            {
                IdentitiesKeys.Add(identity.Id);
            }
        }
    }
}
