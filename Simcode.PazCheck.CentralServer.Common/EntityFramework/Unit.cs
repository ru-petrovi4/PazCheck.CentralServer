#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Unit : Identifiable<int>
    {
        [Attr]
        public string Title { get; set; } = @"";

        [Attr]
        public string Desc { get; set; } = @"";

        [HasMany]
        [InverseProperty("Unit")] // Because ActiveProject property exists.
        public List<Project> Projects { get; set; } = new();

        [HasOne]
        public Project? ActiveProject { get; set; }        

        //[Attr(PublicName="loadeddate")]
        //public DateTime LoadedDate { get; set; }

        //[HasMany(PublicName = "logs")]
        //public List<Log> Logs { get; set; } = new();

        //[HasMany(PublicName = "results")]
        //public List<Result> Results { get; set; } = new();

        [HasMany]
        public List<Section> Sections { get; set; } = new();
    }
}
