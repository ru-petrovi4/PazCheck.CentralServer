using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Frozen;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public static partial class PazCheckDbHelper
    {
        #region public functions      

        public static PcObject GetRootPcObject(PcObject pcObject)
        {
            PcObject? local_PcObject = pcObject;
            while (local_PcObject.Parent != null)
            {
                local_PcObject = local_PcObject.Parent;
            }
            return local_PcObject;
        }

        public static bool CheckBasePcObject(Common.Serialization.BasePcObject serializationBasePcObject, DbCache dbCache)
        {
            if (!dbCache.BasePcObjectsDictionary1.TryGetValue(serializationBasePcObject.Unit + "." + serializationBasePcObject.Identifier, out Common.EntityFramework.BasePcObject? existingBasePcObject))
                return false;

            if (serializationBasePcObject.Params is not null)
            { 
                var existingParamsDicitonary = existingBasePcObject.ParamsDictionary;
                var existingJournalParamsDicitonary = existingBasePcObject.JournalParams.ToDictionary(jp => jp.ParamName, jp => jp.MetadataFields);
                if (!serializationBasePcObject.Params.All(p => 
                        {
                            if (!p.Name.StartsWith(PazCheckConstants.ParamNamePrefix_Data, StringComparison.InvariantCultureIgnoreCase))
                                return existingParamsDicitonary.TryGetValue(p.Name) == p.Value;
                            else
                                return existingJournalParamsDicitonary.GetValueOrDefault(p.Name) == p.Value;
                        }))
                    return false;
            }      

            return true;
        }

        public static bool CheckPcObject(Common.Serialization.PcObject serializationPcObject, DbCache dbCache)
        {
            if (!dbCache.PcObjectsDictionary1.TryGetValue(serializationPcObject.Unit + "." + serializationPcObject.Identifier, out Common.EntityFramework.PcObject? existingPcObject))
                return false;            

            if (serializationPcObject.PcObjectEvents is not null && serializationPcObject.PcObjectEvents.Count > 0)
                return false;

            if (serializationPcObject.JournalParamValuesCollections is not null && serializationPcObject.JournalParamValuesCollections.Count > 0)
                return false;

            if (serializationPcObject.Params is not null)
            {
                var existingParamsDicitonary = existingPcObject.ParamsDictionary;
                var existingJournalParamsDicitonary = existingPcObject.JournalParams.ToDictionary(jp => jp.ParamName, jp => jp.MetadataFields);
                if (!serializationPcObject.Params.All(p =>
                        {
                            if (!p.Name.StartsWith(PazCheckConstants.ParamNamePrefix_Data, StringComparison.InvariantCultureIgnoreCase))
                                return existingParamsDicitonary.TryGetValue(p.Name) == p.Value;
                            else
                                return existingJournalParamsDicitonary.GetValueOrDefault(p.Name) == p.Value;
                        }))
                    return false;
            }          

            return true;
        }

        public static List<PcObject> GetPcObjectAndChildren(PcObject pcObject)
        {
            List<PcObject> children = new() { pcObject };

            foreach (var childPcObject in pcObject.Children.Where(po => po._IsDeleted == false)) // Entity Framework issues.
            {
                children.AddRange(GetPcObjectAndChildren(childPcObject));
            }

            return children;
        }

        public static List<PcObject> GetChildrenForSafetyIndex(PcObject pcObject)
        {
            List<PcObject> childrenForSafetyIndex = new();

            foreach (var childPcObject in pcObject.Children.Where(po => po._IsDeleted == false)) // Entity Framework issues.
            {
                if (childPcObject.BasePcObject.JournalParams.Any(jp => String.Equals(jp.ParamName, PazCheckConstants.ParamNamePrefix_Data + PazCheckConstants.ParamName_SafetyIndex, StringComparison.InvariantCultureIgnoreCase)) ||
                        childPcObject.JournalParams.Any(jp => String.Equals(jp.ParamName, PazCheckConstants.ParamNamePrefix_Data + PazCheckConstants.ParamName_SafetyIndex, StringComparison.InvariantCultureIgnoreCase)))
                    childrenForSafetyIndex.Add(childPcObject);
                else
                    childrenForSafetyIndex.AddRange(GetChildrenForSafetyIndex(childPcObject));
            }

            return childrenForSafetyIndex;
        }

        public static double GetSafetyIndex(PcObject pcObject)
        {
            var safetyIndex_JournalParamValuesCollection = pcObject.JournalParamValuesCollections.FirstOrDefault(vc => vc.ParamName.Contains(PazCheckConstants.ParamName_SafetyIndex, StringComparison.InvariantCultureIgnoreCase));
            if (safetyIndex_JournalParamValuesCollection is null || safetyIndex_JournalParamValuesCollection.CurrentValue is null)
                return -100.0;
            return (double)safetyIndex_JournalParamValuesCollection.CurrentValue.Value;
        }

        public static string GetEventKind(int type, string conditionCategory, bool newValue)
        {
            string eventKind = GetEventTypeString(type);                 
            eventKind += "|" + GetConditionCategoryPure(conditionCategory);
            eventKind += "|" + new Any(newValue).ValueAsString(false);
            return eventKind;
        }

        public static string GetEventTypeString(int type)
        { 
            switch (type)
            {
                case PazCheckConstants.ResultEventType_Cause:
                    return @"Cause";
                case PazCheckConstants.ResultEventType_Effect:
                    return @"Effect";                    
                case PazCheckConstants.ResultEventType_AdditionalCause:
                    return @"AdditionalCause";                    
                case PazCheckConstants.ResultEventType_AdditionalEffect:
                    return @"AdditionalEffect";                    
                case PazCheckConstants.ResultEventType_CellLogicOutput:
                    return @"CellLogicOutput";                    
                case PazCheckConstants.ResultEventType_ColumnLogicOutput:
                    return @"ColumnLogicOutput";                    
                default:
                    return @"Unknown";                    
            }
        }

        public static string GetConditionCategoryPure(string conditionCategory)
        {
            if (conditionCategory.StartsWith(@"!"))
                return conditionCategory.Substring(1).Trim();
            else
                return conditionCategory.Trim();
        }

        public static string GetStateName(string conditionCategory)
        {
            var index = conditionCategory.IndexOf('[');
            if (index >= 0 && conditionCategory.EndsWith(@"]"))
                return conditionCategory.Substring(
                        index + 1,
                        conditionCategory.Length - index - 2).Trim();
            else
                return @"";
        }        

        #endregion
    }
}