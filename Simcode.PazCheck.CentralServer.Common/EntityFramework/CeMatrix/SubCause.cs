// Copyright (c) 2021
// All rights reserved by Simcode
#nullable enable
using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class SubCause : VersionEntityBase
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
        ///     <example>PVHighHigh</example>
        ///     <example>ALARM=����</example>        
        /// </summary>
        [Attr]
        public string TagConditionString { get; set; } = @"";
        
        [Attr]
        public string TagConditionString_SymbolToDisplay { get; set; } = @"";

        [Attr]
        public string CustomFieldValues { get; set; } = @"";
        
        [HasOne]
        public Cause Cause { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => Cause;
    }
}

