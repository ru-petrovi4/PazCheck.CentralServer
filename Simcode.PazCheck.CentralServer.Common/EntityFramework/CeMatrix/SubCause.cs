// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class SubCause : VersionEntity
    {
        /// <summary>
        ///     ���������� ����� ������ �������
        /// </summary>
        [Attr(PublicName = "num")]
        public int Num { get; set; }

        [Attr(PublicName = "name")]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     ��������� � ������� �� ����������
        /// </summary>
        [Attr(PublicName = "state")]
        public string TagConditionString { get; set; } = @"";        

        [Attr(PublicName = "descr")]
        public string CustomFieldValues { get; set; } = @"";
        
        [HasOne(PublicName = "cause")]
        public Cause Cause { get; set; } = null!;
    }
}

