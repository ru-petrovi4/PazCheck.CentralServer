// Copyright (c) 2021
// All rights reserved by Simcode

using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class TagEvent : VersionEntity
    {
        [Attr]
        public DateTime EventTimeUtc { get; set; }

        [Attr]
        public string Value { get; set; } = @"";
    }
}
