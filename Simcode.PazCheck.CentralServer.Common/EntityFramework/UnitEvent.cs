using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Ssz.Utils;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     ??????? ??????????????? ?????????
    /// </summary>
    [Resource]
    public class UnitEvent : Identifiable<int>
    {
        /// <summary>
        ///     ????? ???????
        /// </summary>
        [Attr]
        public DateTime EventTimeUtc { get; set; }

        /// <summary>
        ///     ??? ????
        /// </summary>
        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     TagCondition_Identifier[=TagCondition_Value]
        /// </summary> 
        [Attr]
        public string ConditionString { get; set; } = @"";

        /// <summary>
        ///     ???????? ????? ????????, ?????????? ??? ?? ??????????.
        /// </summary>
        [Attr]
        public bool? ConditionIsActive { get; set; }        
        
        /// <summary>
        ///     ???????? ??????? ?? ????? ????????? ??????. Url encoded name-values collection
        /// </summary>
        public string OriginalEvent { get; set; } = @"";

        /// <summary>
        ///     ????????? ???????. 0 - Journal (J); 1 - Low (L); 2 - High (H); 3 - Urgent (U)
        /// </summary>
        [Attr]
        public int Priority { get; set; }
        
        /// <summary>
        ///     ???????? ??????? ??? ????????????
        /// </summary>
        [Attr]        
        public string Message { get; set; } = @"";

        [Attr]
        [NotMapped]
        public Dictionary<string, string?> OriginalEventDictionary
        {
            get
            {
                return NameValueCollectionHelper.Parse(OriginalEvent);
            }
            set
            {
                OriginalEvent = NameValueCollectionHelper.GetNameValueCollectionString(value);
            }
        }

        public string GetFullConditionString() => TagName + "." + ConditionString;
    }
}
