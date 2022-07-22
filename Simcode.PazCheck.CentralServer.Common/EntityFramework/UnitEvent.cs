using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

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
        ///     TagConditionIdentifier[=TagConditionValue]
        /// </summary>
        [Attr]
        public string TagConditionString { get; set; } = @"";

        [Attr]
        public bool ConditionIsActive { get; set; }

        [Attr]
        public string EventSource { get; set; } = @"";
        
        /// <summary>
        ///     JSON string, name-values collection
        /// </summary>
        public string OriginalEvent { get; set; } = @"";

        [Attr]
        [NotMapped]
        public Dictionary<string, string> OriginalEventDictionary
        {
            get
            {
                return JsonFieldsHelper.GetDictionary(OriginalEvent);
            }
            set
            {
                OriginalEvent = JsonFieldsHelper.SetDictionary(value);
            }
        }
    }
}
