#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;


namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Project : VersionEntity
    {
        [Attr(PublicName = "name")] 
        public string Title { get; set; } = @"";

        [Attr(PublicName = "descr")] 
        public string Desc { get; set; } = @"";

        [Attr(PublicName = "comment")]
        public string Comment { get; set; } = @"";

        [Attr(PublicName = "lastchanged")]
        public DateTime LastChanged { get; set; } = DateTime.UtcNow;

        [HasOne(PublicName = "unit")]        
        public Unit Unit { get; set; } = null!;

        [HasMany(PublicName = "diagrams")] 
        public List<CeMatrix> CeMatrices { get; set; } = new();

        [HasMany(PublicName = "tags")] 
        public List<Tag> Tags { get; set; } = new();                
    }
}
