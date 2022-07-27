using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Ssz.Utils;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class UnitEvent : Identifiable<int>
    {
        [Attr]
        public DateTime EventTimeUtc { get; set; }

        [Attr]
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     TagCondition_Identifier[=TagCondition_Value]
        /// </summary>        
        public string ConditionString { get; set; } = @"";

        /// <summary>
        ///     Состояне стало активным, неактивным или не изменилось.
        /// </summary>
        [Attr]
        public bool? ConditionIsActive { get; set; }        
        
        /// <summary>
        ///     Url encoded name-values collection
        /// </summary>
        public string OriginalEvent { get; set; } = @"";

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
