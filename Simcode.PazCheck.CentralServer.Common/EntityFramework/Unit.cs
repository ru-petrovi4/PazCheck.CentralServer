#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Unit : Identifiable<int>
    {
        [Attr(PublicName = "name")]
        public string Title { get; set; } = @"";

        [Attr(PublicName = "descr")]
        public string Desc { get; set; } = @"";

        [HasMany(PublicName = "projects")]
        [InverseProperty("Unit")]
        public List<Project> Projects { get; set; } = new();

        [HasOne(PublicName = "activeproject")]
        public Project? ActiveProject { get; set; }        

        //[Attr(PublicName="loadeddate")]
        //public DateTime LoadedDate { get; set; }

        //[HasMany(PublicName = "logs")]
        //public List<Log> Logs { get; set; } = new();

        //[HasMany(PublicName = "results")]
        //public List<Result> Results { get; set; } = new();

        [HasMany(PublicName = "sections")]
        public List<Section> Sections { get; set; } = new();
    }
}
