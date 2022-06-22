// Copyright (c) 2021
// All rights reserved by Simcode

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Ssz.Utils.DataAccess;


namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Section : Identifiable<int>, IValueSubscription
    {
        [Attr]
        public string Title { get; set; } = @"";

        [Attr]
        public int Level { get; set; } = 0;

        [Attr]
        public string Calculation { get; set; } = @"";

        [HasMany]
        public List<Section> Children { get; set; } = new();

        [HasOne]
        [ForeignKey("ParentSectionId")]
        public Section? Parent { get; set; }

        [Attr]
        public int AlarmLevel { get; set; } = 0;

        [Attr]
        public double AlarmOpacity { get; set; } = 0;

        [Attr]
        public double K { get; set; } = 1;

        [HasOne]        
        public Unit Unit { get; set; } = null!;

        [NotMapped]
        public string MappedElementIdOrConst { get; set; } = @"";

        public void Update(ValueStatusTimestamp vst)
        {
            AlarmLevel = vst.Value.ValueAsInt32(false);
        }
    }
}
