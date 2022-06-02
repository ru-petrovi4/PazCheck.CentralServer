// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class SubCause : Identifiable<int>
    {
        [Attr(PublicName = "num")]
        public int Num { get; set; } // Порядковый номер внутри причины
        [Attr(PublicName = "name")]
        public string Name { get; set; } = @""; //Имя тега
        [Attr(PublicName = "descr")]
        public string Descr { get; set; } = @""; //Некая описательная часть
        [Attr(PublicName = "state")]
        public string State { get; set; } = @""; //Положение в котором он срабатыает                        
        [HasOne(PublicName = "identity")]
        public Identity Identity { get; set; } = null!;
        [HasOne(PublicName = "cause")]
        public Cause Cause { get; set; } = null!;
    }
}

