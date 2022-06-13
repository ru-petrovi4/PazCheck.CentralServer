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
        [Attr]
        public int Num { get; set; }

        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     ��������� � ������� �� ����������
        /// </summary>
        [Attr]
        public string TagConditionString { get; set; } = @"";        

        [Attr]
        public string CustomFieldValues { get; set; } = @"";
        
        [HasOne]
        public Cause Cause { get; set; } = null!;
    }
}

