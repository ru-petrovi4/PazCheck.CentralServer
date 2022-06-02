// Copyright (c) 2021
// All rights reserved by Simcode

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Ssz.Utils.DataAccess;

#nullable enable

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Section : Identifiable<int>, IValueSubscription
    {
        [Attr(PublicName = "name")]
        public string Name { get; set; } = @"";
        [Attr(PublicName = "level")]
        public int Level { get; set; } = 0;

        [HasMany(PublicName = "children")]
        public List<Section> Children { get; set; } = new();
        [HasOne(PublicName="parent")]
        [ForeignKey("ParentId")]
        public Section? Parent { get; set; }
        [Attr(PublicName = "alarmlevel")]
        public int AlarmLevel { get; set; } = 0;
        [Attr(PublicName = "k")]
        public double K { get; set; } = 1;
        [Attr(PublicName = "width")]
        public double Width { get; set; }
        [Attr(PublicName = "height")]
        public double Height{ get; set; }
        public int UnitId { get; set; }

        [HasOne(PublicName = "unit")]
        [ForeignKey("UnitId")]
        public Unit Unit { get; set; } = null!;
        public string MappedElementIdOrConst { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public void Update(ValueStatusTimestamp vst)
        {
            AlarmLevel = vst.Value.ValueAsInt32(false);
        }
    }
}
