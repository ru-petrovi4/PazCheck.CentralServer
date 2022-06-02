#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;


namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Project : Identifiable<int>
    {
        [Attr(PublicName = "name")] 
        public string Name { get; set; } = @"";

        [Attr(PublicName = "descr")] 
        public string Descr { get; set; } = @"";        

        [HasOne(PublicName = "unit")]        
        public Unit Unit { get; set; } = null!;

        [HasMany(PublicName = "diagrams")] 
        public List<Diagram> Diagrams { get; set; } = new();

        [HasMany(PublicName = "tags")] 
        public List<Tag> Tags { get; set; } = new();

        [HasMany(PublicName = "causes")] 
        public List<Cause> Causes { get; set; } = new();

        [HasMany(PublicName = "effects")] 
        public List<Effect> Effects { get; set; } = new();

        [Attr(PublicName = "lastchanged")] 
        public DateTime LastChanged { get; set; } = DateTime.UtcNow;

        [HasOne(PublicName = "byuser")] 
        public Simuser? ByUser { get; set; }

        [Attr(PublicName = "comment")] 
        public string Comment { get; set; } = @"";

        //[NotMapped]
        //[JsonIgnore]
        //public List<Intersection> Intersections
        //{
        //    get
        //    {
        //        var retList = new List<Intersection>();
        //        foreach (var diagram in this.Diagrams)
        //        {
        //            retList.AddRange(diagram.Intersections);
        //        }

        //        return retList;
        //    }
        //}
    }
}
