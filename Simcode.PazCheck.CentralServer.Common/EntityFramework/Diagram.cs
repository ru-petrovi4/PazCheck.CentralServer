using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Diagram : Identifiable<int>
    {
        [Attr(PublicName="name")]
        public string Name { get; set; } = @"";

        public int ProjectId { get; set; }

        [HasOne(PublicName="project")]
        public Project Project { get; set; }

        //[HasManyThrough(nameof(Diagramcauses))]
        [HasMany(PublicName = "causes")]
        public List<Cause> Causes { get; set; }

        //[HasManyThrough(nameof(Diagrameffects))]
        [HasMany(PublicName = "effects")]
        public ICollection<Effect> Effects { get; set; }

        [HasMany(PublicName="intersections")]
        public List<Intersection> Intersections { get; set; }

        //For import/export
        [NotMapped]
        public List<int> CauseKeys { get; set; }

        [NotMapped]
        public List<int> EffectKeys { get; set; }

        [Attr(PublicName = "causecustomfieldnames")]
        public string CauseCustomFieldNames { get; set; } = @"";

        [Attr(PublicName = "effectcustomfieldnames")]
        public string EffectCustomFieldNames { get; set; } = @"";

        public Diagram()
        {
            Causes = new List<Cause>();
            Effects = new List<Effect>();
            CauseKeys = new List<int>();
            EffectKeys = new List<int>();            
            Intersections = new List<Intersection>();
        }

        public void MapKeys()
        {
            CauseKeys.Clear();
            EffectKeys.Clear();
            foreach (var cause in Causes)
            {
                CauseKeys.Add(cause.Id);
            }

            foreach (var effect in Effects)
            {
                EffectKeys.Add(effect.Id);
            }
        }
    }
}
