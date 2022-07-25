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
        ///     Порядковый номер внутри причины
        /// </summary>
        [Attr]
        public int Num { get; set; }

        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     TagCondition_Identifier[=TagCondition_Value]   
        ///     <example>PVHighHigh</example>
        ///     <example>ALARM=ЗАКР</example>        
        /// </summary>
        [Attr]
        public string ConditionString { get; set; } = @"";
        
        [Attr]
        public string ConditionString_SymbolToDisplay { get; set; } = @"";

        [Attr]
        public string CustomFieldHeader { get; set; } = @"";
        
        [HasOne]
        public Cause Cause { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => Cause;

        /// <summary>
        ///     TAG.TagCondition_Identifier[=TagCondition_Value]
        /// </summary>
        public string GetTagNameAndConditionString() => TagName + "." + ConditionString;

        /// <summary>
        ///     TAG.TagConditionString_SymbolToDisplay
        /// </summary>
        public string GetTagNameAndConditionString_SymbolToDisplay() => TagName + "." + ConditionString_SymbolToDisplay;
    }
}

